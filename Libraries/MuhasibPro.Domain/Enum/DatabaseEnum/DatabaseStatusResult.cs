namespace MuhasibPro.Domain.Enum.DatabaseEnum
{
    public enum DatabaseStatusResult
    {
        Healty,             // Veritabanı sağlıklı
        RequiredUpdating,   // Güncelleme gerekiyor (Veya ilk kurulum bekliyor)
        DatabaseNotFound,   // Veritabanı dosyası fiziksel olarak yok
        ConnectionFailed,   // Dosya var ama bağlantı kurulamıyor (Kilitli veya bozuk)
        InvalidSchema,      // Bağlantı var ama tablo yapısı hatalı
        UnknownError,        // Beklenmedik bir Exception durumu       
        RestoreCompleted,   // Yedekten geri yüklendi
        FutureSchema,       // Veritabanı daha yeni bir sürümle yazılmış (fail-closed, Faz 6.91-C)
        
        
        
    }
}
