using System.Collections.Concurrent;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.DTOModel;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.Infrastructure.Security;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Domain.Entities;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Exceptions;
using MuhasibPro.Domain.Models;

namespace MuhasibPro.Business.Services.SistemServices.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticator _authenticator;
        private readonly IBitmapToolsService _bitmapTools;
        private readonly IMessageService _messageService;
        private readonly IUserRepository _userRepository;
        private HesapModel _currentAccount;
        private readonly ConcurrentDictionary<string, (int count, DateTime firstAttempt, DateTime? lockoutUntil)> _loginAttempts = new();
        private readonly IIdentitySettingsProvider _identitySettings;

        public HesapModel CurrentAccount
        {
            get => _currentAccount;
            private set
            {
                if (_currentAccount != value)
                {
                    _currentAccount = value;
                    StateChanged?.Invoke();
                    _messageService.Send(this, "AuthenticationChanged", IsAuthenticated);
                }
            }
        }

        public bool IsAuthenticated => CurrentAccount != null;
        public string GetCurrentUsername => CurrentAccount?.KullaniciModel.KullaniciAdi ?? "App";
        public long GetCurrentUserId => CurrentAccount?.KullaniciId ?? -1;
        public event Action StateChanged;

        public AuthenticationService(IAuthenticator authenticator, IBitmapToolsService bitmapTools, IMessageService messageService, ModelFactory modelFactory, IUserRepository userRepository = null!, IIdentitySettingsProvider identitySettings = null!)
        {
            _authenticator = authenticator;
            _bitmapTools = bitmapTools;
            _messageService = messageService;
            _userRepository = userRepository;
            _identitySettings = identitySettings;
        }

        private async Task<IdentitySettings> LoadIdentitySettingsAsync()
        {
            try
            {
                if (_identitySettings != null)
                    return await _identitySettings.GetAsync();
            }
            catch { /* model varsayılanı */ }
            return new IdentitySettings();
        }

        private bool IsLockedOut(string username, out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;
            if (_loginAttempts.TryGetValue(username.ToLowerInvariant(), out var value) && value.lockoutUntil.HasValue)
            {
                if (DateTime.UtcNow < value.lockoutUntil.Value)
                {
                    remaining = value.lockoutUntil.Value - DateTime.UtcNow;
                    return true;
                }
                _loginAttempts.TryRemove(username.ToLowerInvariant(), out _);
            }
            return false;
        }

        private void RecordFailedAttempt(string username, IdentitySettings settings)
        {
            var key = username.ToLowerInvariant();
            var orAdd = _loginAttempts.GetOrAdd(key, (0, DateTime.UtcNow, null));
            var utcNow = DateTime.UtcNow;
            if ((utcNow - orAdd.firstAttempt).TotalMinutes > settings.AttemptWindowMinutes)
            {
                _loginAttempts[key] = (1, utcNow, null);
                return;
            }
            var num = orAdd.count + 1;
            DateTime? item = num >= settings.MaxFailedAttempts
                ? utcNow.Add(TimeSpan.FromMinutes(settings.LockoutMinutes))
                : null;
            _loginAttempts[key] = (num, orAdd.firstAttempt, item);
        }

        private void ClearAttempts(string username) => _loginAttempts.TryRemove(username.ToLowerInvariant(), out _);

        public async Task Login(string username, string password)
        {
            var kimlik = await LoadIdentitySettingsAsync();
            if (IsLockedOut(username, out var remaining))
                throw new InvalidOperationException($"Çok fazla hatalı deneme. Lütfen {Math.Ceiling(remaining.TotalSeconds)} saniye sonra tekrar deneyin.");
            try
            {
                var kullanici = await _authenticator.Login(username, password);
                if (kullanici == null) throw new Exception("Kullanıcı bulunamadı");
                ClearAttempts(username);
                CurrentAccount = new HesapModel
                {
                    KullaniciModel = CreateKullaniciModel(kullanici, true),
                    KullaniciId = kullanici.Id,
                    SonGirisTarihi = DateTime.UtcNow
                };
            }
            catch (InvalidPasswordException)
            {
                if (_userRepository != null)
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.ParolaHash != null && user.ParolaHash.StartsWith("PBKDF2$") && LegacyPbkdf2Verifier.Verify(password, user.ParolaHash))
                    {
                        ClearAttempts(username);
                        CurrentAccount = new HesapModel
                        {
                            KullaniciModel = CreateKullaniciModel(user, true),
                            KullaniciId = user.Id,
                            SonGirisTarihi = DateTime.UtcNow
                        };
                        return;
                    }
                }
                RecordFailedAttempt(username, kimlik);
                throw;
            }
            catch (UserNotFoundException)
            {
                RecordFailedAttempt(username, kimlik);
                throw;
            }
        }

        public void Logout()
        {
            _authenticator.Logout();
            CurrentAccount = null!;
        }

        public async Task<RegistrationResult> Register(string email, string username, string password, string confirmPassword)
        {
            try
            {
                return await _authenticator.Register(email, username, password, confirmPassword).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı kaydedilmedi", ex);
            }
        }

        private KullaniciModel CreateKullaniciModel(Kullanici source, bool includeAllFields = false)
        {
            try
            {
                return ModelFactory.CreateModelFromEntity<KullaniciModel, Kullanici>(
                    source,
                    includeAllFields,
                    (model, entity, includes) =>
                    {
                        model.Adi = entity.Adi;
                        model.Soyadi = entity.Soyadi;
                        model.Eposta = entity.Eposta;
                        model.KullaniciAdi = entity.KullaniciAdi;
                        model.Telefon = entity.Telefon;
                        model.ResimOnizleme = entity.Resim;
                        model.ResimOnizlemeSource = _bitmapTools.CreateLazyImageLoader(entity.ResimOnizleme);
                        model.Resim = entity.Resim;
                        model.ResimSource = entity.Resim;
                        // Birden çok firma rolü varsa yönetici satırı tercih edilir (ilk satır
                        // admin olmayabilir — aksi halde yönetici kilitli kalır).
                        var roller = entity.KullaniciFirmaRoller;
                        var kfr = roller?.FirstOrDefault(x => x?.Rol?.RolTip == KullaniciRolTip.Yönetici)
                            ?? roller?.FirstOrDefault();
                        if (kfr?.Rol != null)
                        {
                            model.Rol = CreateKullaniciRol(kfr.Rol);
                            model.RolId = kfr.RolId;
                        }
                        else if (source.Id == KullaniciSabitleri.SeedYoneticiId)
                        {
                            // Seed'de firma-rol satırı üretilmez (firma-bağımlı); yönetici kilidi
                            // bootstrap kuralıyla aynı: seed yöneticisi "Yönetici" sayılır (Oturum 129/130).
                            model.Rol = new KullaniciRolModel { RolAdi = "Yönetici", RolTip = KullaniciRolTip.Yönetici };
                        }
                        if (includes)
                        {
                            model.Resim = source.Resim;
                            model.ResimSource = _bitmapTools.CreateLazyImageLoader(source.Resim);
                        }
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı oluşturulurken hata oluştu", ex);
            }
        }

        private KullaniciRolModel CreateKullaniciRol(KullaniciRol source, bool includesAllFields = false)
        {
            try
            {
                return ModelFactory.CreateModelFromEntity<KullaniciRolModel, KullaniciRol>(
                    source,
                    includesAllFields,
                    (model, entity, include) =>
                    {
                        model.Aciklama = entity.Aciklama;
                        model.RolAdi = entity.RolAdi;
                        model.RolTip = entity.RolTip;
                    });
            }
            catch
            {
                throw new Exception("Kullanıcı Rol'ü oluşturulurken hata oluştu");
            }
        }
    }
}
