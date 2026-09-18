using MuhasibPro.Business.Contracts.SistemServices.Authentication;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.DTOModel.SistemModel;
using MuhasibPro.Domain.Enum;
using MuhasibPro.ViewModels.Infrastructure.Common;
using MuhasibPro.ViewModels.Infrastructure.ViewModels;
using System.Windows.Input;

namespace MuhasibPro.ViewModels.ViewModels.KullaniciYonetimi
{
    public class KullaniciListViewModel : GenericListViewModel<KullaniciModel>
    {
        private readonly IKullaniciService _kullaniciService;
        private long _firmaId;

        public KullaniciListViewModel(ICommonServices commonServices, IKullaniciService kullaniciService)
            : base(commonServices)
        {
            _kullaniciService = kullaniciService;
        }

        public string Header => "Kullanıcılar";

        public async Task LoadAsync(long firmaId)
        {
            _firmaId = firmaId;

            await ExecuteActionAsync(
                action: async () => await RefreshAsync(),
                startMessage: $"{Header} yükleniyor",
                startMessageType: StatusMessageType.Refreshing,
                successMessage: $"{Header} yüklendi",
                errorMessage: $"{Header} yükleme hatası");
        }

        public void Subscribe()
        {
            MessageService.Subscribe<KullaniciListViewModel>(this, OnMessage);
            MessageService.Subscribe<KullaniciDetailsViewModel>(this, OnMessage);
        }

        public void Unsubscribe() { MessageService.Unsubscribe(this); }

        public ICommand OpenInNewViewCommand => new RelayCommand(OnOpenInNewView);

        private async void OnOpenInNewView()
        {
            if (SelectedItem != null)
            {
                await NavigationService.CreateNewViewAsync<KullaniciDetailsViewModel>(
                    new KullaniciDetailsArgs { KullaniciId = SelectedItem.Id, FirmaId = _firmaId },
                    customTitle: "Kullanıcı");
            }
        }

        protected override async Task LoadDataAsync()
        {
            var sonuc = await _kullaniciService.GetKullanicilarWithRolAsync(_firmaId);
            var tumu = sonuc.Success && sonuc.Data != null ? sonuc.Data : new List<KullaniciModel>();

            ItemsCount = tumu.Count;
            Items = tumu.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            await ContextService.RunAsync(
                () =>
                {
                    var oncekiSecili = SelectedItem;
                    long preservedId = oncekiSecili?.Id ?? 0;
                    ItemsSource.Clear();
                    foreach (var item in Items)
                    {
                        ItemsSource.Add(item);
                    }
                    if (IsMultipleSelection || ItemsSource.Count == 0)
                        return;
                    SelectedItem = preservedId > 0
                        ? ItemsSource.FirstOrDefault(i => i.Id == preservedId) ?? ItemsSource.First()
                        : ItemsSource.First();
                });
        }

        public async Task<bool> RefreshAsync()
        {
            try
            {
                await LoadDataAsync();
                NotifyPropertyChanged(nameof(Title));
                return true;
            }
            catch (Exception ex)
            {
                StatusError($"{Header} listesi yenilenirken beklenmeyen hata");
                await LogSistemExceptionAsync(Header, "Yenile", ex);
                return false;
            }
        }

        protected override void OnNew()
        {
            if (IsMainWindow)
                _ = NavigationService.CreateNewViewAsync<KullaniciDetailsViewModel>(
                    new KullaniciDetailsArgs { FirmaId = _firmaId },
                    customTitle: "Yeni Kullanıcı");
            else
                NavigationService.Navigate<KullaniciDetailsViewModel>(
                    new KullaniciDetailsArgs { FirmaId = _firmaId });
            StatusReady();
        }

        protected override async void OnRefresh()
        {
            await ExecuteActionAsync(
                action: async () => await RefreshAsync(),
                startMessage: $"{Header} yenileniyor...",
                startMessageType: StatusMessageType.Refreshing,
                successMessage: $"{Header} yenilendi",
                errorMessage: $"Hata! {Header} alınamadı");
        }

        protected override async void OnDeleteSelection()
        {
            StatusReady();
            if (!await DialogService.ShowAsync(
                "Silmeyi Onayla",
                $"Seçili {Header.ToLowerInvariant()}ı silmek istediğinize emin misiniz?",
                "Evet",
                "İptal"))
                return;

            var silinecekler = new List<KullaniciModel>();
            if (SelectedItems != null)
                silinecekler.AddRange(SelectedItems);
            else if (SelectedIndexRanges != null)
                foreach (var range in SelectedIndexRanges)
                    for (int i = range.Index; i < range.Index + range.Length && i < Items.Count; i++)
                        silinecekler.Add(Items[i]);

            if (silinecekler.Count == 0)
                return;

            int sayi = 0;
            try
            {
                StartProgressWithPercent($"{silinecekler.Count} kullanıcı siliniyor...");
                foreach (var model in silinecekler)
                {
                    var sonuc = await _kullaniciService.DeleteKullaniciAsync(model.Id);
                    if (sonuc.Success)
                        sayi++;
                }
            }
            catch (Exception ex)
            {
                StatusError($"{Header} silinirken beklenmeyen hata");
                await LogSistemExceptionAsync(Header, "Sil", ex);
            }
            finally
            {
                await RefreshAsync();
                SelectedIndexRanges = null;
                SelectedItems = null;
                StopProgress();
            }

            if (sayi > 0)
                StatusActionMessage($"{sayi} {Header.ToLowerInvariant()} silindi", StatusMessageType.Deleting, autoHide: -1);
        }

        private async void OnMessage(ViewModelBase sender, string message, object args)
        {
            switch (message)
            {
                case EntityEvents.NewItemSaved:
                case EntityEvents.ItemChanged:
                case EntityEvents.ItemDeleted:
                case EntityEvents.ItemsDeleted:
                case EntityEvents.ItemRangesDeleted:
                    await ContextService.RunAsync(
                        async () =>
                        {
                            await RefreshAsync();
                        });
                    break;
            }
        }
    }
}
