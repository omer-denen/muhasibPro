using MuhasibPro.Business.DTOModel.SistemModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell.Tenant
{
    /// <summary>Kural 13: Veritabanı Güncelleme sayfasının yardım maddeleri (içerik sayfayla yaşar).</summary>
    internal static class TenantDatabaseUpdateYardim
    {
        internal const string YardimAnahtari = "TenantDatabaseUpdate";
        internal const string YardimBasligi = "Veritabanı Güncelleme — Yardım";

        public static List<YardimMaddesiDto> Maddeler() => new()
        {
            new() { Baslik = "Bu sayfa ne yapar?", Aciklama = "Seçili mali dönemin veritabanı şemasını yeni sürüme günceller. Akış üç adımdır: önce güvenlik yedeği alınır, sonra bekleyen göçler uygulanır, en son bağlantı ve şema doğrulanır." },
            new() { Baslik = "Durum kartı (sürüm şeridi)", Aciklama = "Solda mevcut, sağda güncellenecek şema sürümü; yanındaki rozet bekleyen göç sayısını gösterir. Alt satırdaki tek hüküm, bu dönem için ne olacağını özetler." },
            new() { Baslik = "İlerleme çubuğu", Aciklama = "İşlem çalışırken çubuk Yedek ~%30 · Göç ~%70 · Doğrulama ~%100 sırasıyla dolar; üstünde o anki adım yazar. Çubuk geri sarmaz." },
            new() { Baslik = "İşlem akışı", Aciklama = "Yedek → Göç → Doğrulama sırayla işlenir; her adımın altında o anki durum yazar. Doğrulama geçilemezse dördüncü adım 'Geri alma' açılır ve yedekten otomatik dönülür." },
            new() { Baslik = "Değişiklikler", Aciklama = "Hangi tabloya hangi kolonların eklendiğini/güncellendiğini gösterir. 'Kaldırılanlar' satırları yalnızca bilgilendirir — veri silinmez." },
            new() { Baslik = "'Yedekle ve Güncelle'", Aciklama = "İşlemi başlatır. Buton yalnız göç gerekiyorsa ve işlem çalışmıyorken aktiftir. Hata olursa kırmızı bantta neden yazar; geri alma da başarısızsa yedeğin yolu verilir (elle geri yükleme için)." },
            new() { Baslik = "'Çalışma Alanına Geç'", Aciklama = "Doğrulama başarılı olduğunda görünür. Seçimi kaydeder, pencereyi kapatır ve ana ekrandaki 'Devam Et' ile çalışma alanına geçilir." },
            new() { Baslik = "'Geri'", Aciklama = "Hiçbir değişiklik yapmadan firma seçim ekranına döner." },
        };
    }
}
