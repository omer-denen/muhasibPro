# Açılış ve Kurulum Ekranları

## Açılış (Splash) ekranı
Uygulama açılırken logo, "MuhasibPro" başlığı ve "Yeni Nesil Ön Muhasebe" alt başlığı ile açılış ekranı görünür. Altta durum yazısı ("Başlatılıyor…"), ilerleme çubuğu ve yüzde bilgisi yer alır. Başlatma 30 saniyeyi aşarsa "Hata — Uygulama başlatılamadı" penceresi çıkar; "Tamam" sonrası uygulama kapanır.
Etiket: açılış ekranı, splash, başlatılıyor, başlatma hatası

## Açılıştan sonra hangi ekran gelir?
Açılış ekranı duruma göre yönlendirir: sistem veritabanı yoksa kurulum ekranı, bekleyen göç varsa sistem veritabanı yönetimi, geliştirme ortamında otomatik göç gerekiyorsa şema güncelleme ekranı, güncelleme sonrası doğrulama gerekiyorsa doğrulama ekranı; aksi halde giriş ekranı açılır.
Etiket: yönlendirme, giriş ekranı, kurulum, göç

## İlk kurulum ekranı
Sistem veritabanı henüz kurulmadıysa "İlk Kurulum" başlıklı ekran açılır; durum mesajları sırayla "Hazırlanıyor...", "Sistem veritabanı oluşturuluyor...", "Veritabanı dosyası hazırlanıyor...", "Veritabanı doğrulaması yapılıyor...", "Kurulum tamamlandı" şeklinde ilerler. İlerleme yüzdeyle gösterilir; tamamlanınca giriş ekranına dönülür.
Etiket: ilk kurulum, kurulum ekranı, veritabanı oluşturma

## Kurulum başarısız olursa?
Ekranda hata mesajı ve "Tekrar Dene" butonu görünür. "Tekrar Dene" kurulumu yeniden başlatır; sorun sürerse girişten önce sistem veritabanı yönetiminden "Sistemi Analiz Et" veya "Onar" kullanılabilir.
Etiket: kurulum hatası, tekrar dene, onarım

## Şema Güncellemesi ekranı (geliştirme)
Geliştirme ortamında bekleyen göç varsa "Veritabanı Şema Güncellemesi" kartı açılır ve göçler otomatik uygulanır. Mesajlar: "Bekleyen şema güncellemeleri denetleniyor...", "Bekleyen şema güncellemesi yok.", "N şema güncellemesi uygulanıyor (ön yedek alınıyor, göç + doğrulama)...", "N şema güncellemesi uygulandı." Hata olursa "Veritabanı Yönetimi" ve "Tekrar Dene" butonları çıkar.
Etiket: şema güncellemesi, otomatik göç, geliştirme, ön yedek
