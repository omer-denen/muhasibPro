namespace MuhasibPro.Domain.Models.PostUpdateModel
{
    /// <summary>Faz 6.91-D Revizyon 3: güncelleme sonrası doğrulamanın nihai hâli (hero/aksiyon seçimi bu türden türetilir).</summary>
    public enum PostUpdateSonucTuru
    {
        /// <summary>Güncelleme tamamlandı — tüm adımlar temiz (otomatik devam).</summary>
        Temiz = 0,
        /// <summary>Güncelleme tamamlandı; dikkat gerektiren durum(lar) raporlandı (kullanıcı "Devam Et" der).</summary>
        Dikkat = 1,
        /// <summary>Uygulama güncellenemedi (kritik dosya/sürüm) — kullanıcı Login'e döner; damga atılır (tek sefer).</summary>
        UygulamaBasarisiz = 2,
        /// <summary>Sistem.db güncellenemedi/doğrulanamadı — sert blok; giriş kapalı, yönetim ekranına yönlendirir.</summary>
        SistemBasarisiz = 3
    }
}
