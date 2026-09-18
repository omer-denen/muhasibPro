using MuhasibPro.Business.Contracts.SistemServices.AppServices;
using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Business.ModelValidator.SistemValidations;
using MuhasibPro.Domain.Entities.SistemEntity;
using MuhasibPro.Domain.Enum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FluentValidation;

namespace MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi
{
    public class KullaniciDetailsArgs
    {
        public static KullaniciDetailsArgs CreateDefault() => new();

        public long KullaniciId { get; set; }
        public long FirmaId { get; set; }

        public bool IsNew => KullaniciId <= 0;
    }

    public class KullaniciDetailsViewModel : GenericDetailsViewModel<KullaniciModel>
    {
        private readonly IKullaniciService _kullaniciService;
        private readonly IFirmaWithMaliDonemSelectedService _selection;

        public KullaniciDetailsViewModel(
            ICommonServices commonServices,
            IFilePickerService filePickerService,
            IKullaniciService kullaniciService,
            IFirmaWithMaliDonemSelectedService selection) : base(commonServices)
        {
            FilePickerService = filePickerService;
            _kullaniciService = kullaniciService;
            _selection = selection;
        }

        public IFilePickerService FilePickerService { get; }

        private string Header => "Kullanıcı";

        public override string Title => ItemIsNew ? "Yeni Kullanıcı" : (Item?.AdiSoyadi ?? "Kullanıcı");

        public string TitleEdit => Item == null ? "Kullanıcı" : Item.AdiSoyadi;

        public override bool ItemIsNew => Item?.IsNew ?? true;

        public KullaniciDetailsArgs ViewModelArgs { get; private set; }

        public ObservableCollection<KullaniciRolModel> Roller { get; } = new();

        private KullaniciRolModel _seciliRol;
        public KullaniciRolModel SeciliRol { get => _seciliRol; set => Set(ref _seciliRol, value); }

        private string _sifre = string.Empty;
        public string Sifre { get => _sifre; set => Set(ref _sifre, value); }

        private string _sifreTekrar = string.Empty;
        public string SifreTekrar { get => _sifreTekrar; set => Set(ref _sifreTekrar, value); }

        private object _newPictureSource;

        public object NewPictureSource { get => _newPictureSource; set => Set(ref _newPictureSource, value); }

        private long FirmaId => (ViewModelArgs?.FirmaId ?? 0) > 0
            ? ViewModelArgs.FirmaId
            : (_selection.SelectedFirma?.Id ?? 0);

        public async Task LoadAsync(KullaniciDetailsArgs args)
        {
            ViewModelArgs = args ?? KullaniciDetailsArgs.CreateDefault();

            await RolleriYukleAsync();

            if (ViewModelArgs.IsNew)
            {
                Item = new KullaniciModel();
                IsEditMode = true;
            }
            else
            {
                try
                {
                    var sonuc = await _kullaniciService.GetKullanicilarWithRolAsync(FirmaId);
                    var model = sonuc.Success
                        ? sonuc.Data?.FirstOrDefault(k => k.Id == ViewModelArgs.KullaniciId)
                        : null;
                    Item = model ?? new KullaniciModel { Id = ViewModelArgs.KullaniciId, IsEmpty = true };
                }
                catch (Exception ex)
                {
                    StatusError($"{Header} bilgileri yüklenirken beklenmeyen hata");
                    await LogSistemExceptionAsync(Header, $"{Header} Detay", ex);
                }
                NotifyPropertyChanged(nameof(ItemIsNew));
            }

            SenkronizeSeciliRol();
        }

        public async Task RolleriYukleAsync()
        {
            try
            {
                var sonuc = await _kullaniciService.GetRollerAsync();
                Roller.Clear();
                if (sonuc.Success && sonuc.Data != null)
                    foreach (var rol in sonuc.Data)
                        Roller.Add(rol);
            }
            catch (Exception ex)
            {
                await LogSistemExceptionAsync(Header, "Roller", ex);
            }
        }

        public void Unload() => ViewModelArgs.KullaniciId = Item?.Id ?? 0;

        public void Subscribe()
        {
            MessageService.Subscribe<KullaniciDetailsViewModel, KullaniciModel>(this, OnDetailsMessage);
            MessageService.Subscribe<KullaniciListViewModel>(this, OnListMessage);
        }

        public void Unsubscribe() { MessageService.Unsubscribe(this); }

        public override void BeginEdit()
        {
            NewPictureSource = null;
            base.BeginEdit();
            SenkronizeSeciliRol();
        }

        /// <summary>Form rol seçicisini düzenlenen kullanıcının rolüyle eşitler.</summary>
        public void SenkronizeSeciliRol()
        {
            var kaynak = EditableItem ?? Item;
            long rolId = kaynak?.RolId ?? 0;
            SeciliRol = rolId > 0
                ? Roller.FirstOrDefault(r => r.Id == rolId) ?? VarsayilanRol()
                : VarsayilanRol();
        }

        private KullaniciRolModel VarsayilanRol()
            => Roller.FirstOrDefault(r => r.RolTip == KullaniciRolTip.Kullanici) ?? Roller.FirstOrDefault();

        public ICommand EditPictureCommand => new RelayCommand(OnEditPicture);

        private async void OnEditPicture()
        {
            NewPictureSource = null;

            await ExecuteActionAsync(
                action: async () =>
                {
                    var result = await FilePickerService.OpenImagePickerAsync();
                    if (result != null)
                    {
                        EditableItem.Resim = result.ImageBytes;
                        EditableItem.ResimOnizleme = result.ImageBytes;
                        EditableItem.ResimSource = result.ImageSource;
                        EditableItem.ResimOnizlemeSource = result.ImageSource;
                        NewPictureSource = result.ImageSource;

                        StatusActionMessage("Profil resmi güncellendi", StatusMessageType.Success, autoHide: 3);
                    }
                    else
                    {
                        NewPictureSource = null;
                        StatusReady();
                    }
                },
                startMessage: "Resim seçiliyor",
                startMessageType: StatusMessageType.Info);
        }

        protected override async Task<bool> SaveItemAsync(KullaniciModel model)
        {
            try
            {
                long firmaId = FirmaId;
                long rolId = SeciliRol?.Id ?? 0;

                if (model.IsNew)
                {
                    if (Sifre != SifreTekrar)
                    {
                        Bildir("Şifreler eşleşmiyor.");
                        return false;
                    }

                    var sonuc = await _kullaniciService.CreateKullaniciAsync(model, Sifre, firmaId, rolId);
                    if (!sonuc.Success)
                    {
                        Bildir(sonuc.Message);
                        return false;
                    }

                    Sifre = string.Empty;
                    SifreTekrar = string.Empty;
                }
                else
                {
                    var guncelle = await _kullaniciService.UpdateKullaniciAsync(model);
                    if (!guncelle.Success)
                    {
                        Bildir(guncelle.Message);
                        return false;
                    }

                    if (firmaId > 0 && rolId > 0)
                    {
                        var rol = await _kullaniciService.RolAtaAsync(model.Id, firmaId, rolId);
                        if (!rol.Success)
                        {
                            Bildir(rol.Message);
                            return false;
                        }
                    }
                }

                await LogSistemInformationAsync(
                    Header,
                    "Kayıt",
                    $"{Header} başarıyla kaydedildi",
                    $"{Header} '{model.AdiSoyadi}' başarıyla kaydedildi");
                return true;
            }
            catch (Exception ex)
            {
                StatusError($"{Header} kaydedilirken beklenmeyen hata");
                await LogSistemExceptionAsync(Header, "Kayıt", ex);
                return false;
            }
        }

        private void Bildir(string mesaj)
        {
            StatusError(mesaj);
            NotificationService.Show("Hata", mesaj, NotificationType.Danger);
        }

        protected override async Task<bool> DeleteItemAsync(KullaniciModel model)
        {
            try
            {
                var sonuc = await _kullaniciService.DeleteKullaniciAsync(model.Id);
                if (!sonuc.Success)
                {
                    Bildir(sonuc.Message);
                    return false;
                }

                await LogSistemWarningAsync(Header, "Sil", $"{Header} silindi", $"'{TitleEdit}' silindi");
                return true;
            }
            catch (Exception ex)
            {
                StatusError($"{Header} silinirken beklenmeyen hata");
                await LogSistemExceptionAsync(Header, "Sil", ex);
                return false;
            }
        }

        protected override async Task<bool> ConfirmDeleteAsync()
        {
            return await DialogService.ShowAsync(
                "Silme Onayı",
                $"{Header} '{TitleEdit}' silinsin mi? Bu işlem geri alınamaz.",
                "Sil",
                "İptal");
        }

        protected override IEnumerable<AbstractValidator<KullaniciModel>> GetValidationConstraints(KullaniciModel model)
        {
            yield return new KullaniciValidator();
        }

        private async void OnDetailsMessage(KullaniciDetailsViewModel sender, string message, KullaniciModel changed)
        {
            var current = Item;
            if (current != null && changed != null && changed.Id == current.Id)
            {
                switch (message)
                {
                    case EntityEvents.ItemChanged:
                        await ContextService.RunAsync(
                            async () =>
                            {
                                try
                                {
                                    var sonuc = await _kullaniciService.GetKullanicilarWithRolAsync(FirmaId);
                                    var item = sonuc.Success ? sonuc.Data?.FirstOrDefault(k => k.Id == current.Id) : null;
                                    if (item != null)
                                    {
                                        current.Merge(item);
                                        current.NotifyChanges();
                                        NotifyPropertyChanged(nameof(Title));
                                        SenkronizeSeciliRol();
                                    }
                                    if (IsEditMode)
                                        StatusActionMessage(
                                            $"DİKKAT: Bu {Header} başkası tarafından değiştirildi!",
                                            StatusMessageType.Warning,
                                            autoHide: 5);
                                }
                                catch (Exception ex)
                                {
                                    StatusError($"{Header} bilgileri güncellenirken hata");
                                    await LogSistemExceptionAsync(Header, "Değiştirilmiş", ex);
                                }
                            });
                        break;
                    case EntityEvents.ItemDeleted:
                        await OnItemDeletedExternally();
                        break;
                }
            }
        }

        private async Task OnItemDeletedExternally()
        {
            await ContextService.RunAsync(
                () =>
                {
                    CancelEdit();
                    IsEnabled = false;
                    StatusActionMessage($"DİKKAT: Bu {Header} kaydı silinmiş!", StatusMessageType.Warning, autoHide: 5);
                });
        }

        private async void OnListMessage(KullaniciListViewModel sender, string message, object args)
        {
            var current = Item;
            if (current == null)
                return;

            switch (message)
            {
                case EntityEvents.ItemsDeleted:
                    if (args is IList<KullaniciModel> deletedModels && deletedModels.Any(r => r.Id == current.Id))
                        await OnItemDeletedExternally();
                    break;
                case EntityEvents.ItemRangesDeleted:
                    try
                    {
                        var sonuc = await _kullaniciService.GetKullanicilarWithRolAsync(FirmaId);
                        if (sonuc.Success && sonuc.Data != null && sonuc.Data.All(k => k.Id != current.Id))
                            await OnItemDeletedExternally();
                    }
                    catch (Exception ex)
                    {
                        await LogSistemExceptionAsync(Header, $"{Header} kaydı silinmiş!", ex);
                    }
                    break;
            }
        }
    }
}
