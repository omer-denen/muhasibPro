namespace MuhasibPro.Domain.Models.PostUpdateModel
{
    /// <summary>Faz 6.91-D: güncelleme sonrası doğrulama adımının durumu.</summary>
    public enum PostUpdateAdimDurumu
    {
        /// <summary>Henüz başlamadı.</summary>
        Bekliyor = 0,
        /// <summary>Şu an çalışıyor.</summary>
        DevamEdiyor = 1,
        /// <summary>Başarıyla tamamlandı.</summary>
        Basarili = 2,
        /// <summary>Bu ortamda gerekmediği için atlandı.</summary>
        Atlandi = 3,
        /// <summary>Tamamlandı ancak kullanıcı bilgisi gerektiren bir uyarı var.</summary>
        Uyari = 4,
        /// <summary>Başarısız — akışa göre bloklayıcı olabilir.</summary>
        Hata = 5
    }
}
