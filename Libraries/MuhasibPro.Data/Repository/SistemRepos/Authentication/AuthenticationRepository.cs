using Microsoft.AspNetCore.Identity;
using MuhasibPro.Data.Contracts.Repository.SistemRepos.Authentication;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Exceptions;
using MuhasibPro.Domain.Models;
using MuhasibPro.Domain.Utilities.UIDGenerator;

namespace MuhasibPro.Data.Repository.SistemRepos.Authentication;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<Kullanici> _passwordHasher;

    public AuthenticationRepository(IUserRepository userRepository, IPasswordHasher<Kullanici> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Kullanici> Login(string username, string password)
    {
        var kullanici = await _userRepository.GetByUsernameAsync(username).ConfigureAwait(false);
        if (kullanici == null)
            throw new UserNotFoundException(username);
        var passwordResult = _passwordHasher.VerifyHashedPassword(kullanici, kullanici.ParolaHash, password);
        if (passwordResult != PasswordVerificationResult.Success)
            throw new InvalidPasswordException(username, password);
        return kullanici;
    }

    public async Task<RegistrationResult> Register(string email, string username, string password, string confirmPassword)
    {
        var result = RegistrationResult.Success;
        if (password != confirmPassword)
            result = RegistrationResult.PasswordsDoNotMatch;
        if (await _userRepository.GetByEmailAsync(email).ConfigureAwait(false) != null)
            result |= RegistrationResult.EmailAlreadyExists;
        if (await _userRepository.GetByUsernameAsync(username).ConfigureAwait(false) != null)
            result |= RegistrationResult.UsernameAlreadyExists;
        if (result == RegistrationResult.Success)
        {
            var hashedPassword = _passwordHasher.HashPassword(null!, password);
            var kullanici = new Kullanici
            {
                Id = UIDGenerator.GenerateModuleId(UIDModuleType.Sistem),
                Eposta = email,
                KullaniciAdi = username,
                ParolaHash = hashedPassword,
                KayitTarihi = DateTime.UtcNow,
                AktifMi = true
            };
            // Self-registration: kaydı oluşturan, kaydın kendisidir.
            kullanici.KaydedenId = kullanici.Id;
            await _userRepository.AddAsync(kullanici).ConfigureAwait(false);
        }
        return result;
    }
}
