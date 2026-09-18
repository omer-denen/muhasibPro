# Denetim Masası (Ayarlar)

## Denetim Masası'nı nasıl açarım?
Sağ üstteki "Ayarlar" butonu uygulama ayarlarını ayrı bir pencerede (Denetim Masası) açar. Sol menüden bir bölüm seçin; içerik sağdaki alanda görünür. Üstteki "Bir ayar bulun" kutusuna yazarak menüde arama yapabilirsiniz.
Etiket: denetim masası, ayarlar, ayar ara

## Hangi bölümler var?
Sol menüde sırayla: Giriş (hesap/firma/dönem özeti), Görünüm, Güvenlik, Firma, Veritabanı, Mali Dönem, Güncelleme, Yapay Zeka ve (yalnız geliştirme derlemesinde) Geliştirici Araçları bulunur. Yetkiniz olmayan bölümler menüde görünmez.
Etiket: denetim bölümleri, ayar menüsü, yetki

## Giriş bölümü ne gösterir?
Hesap/firma/dönem özetini gösterir. "MuhasibPro" kartı sürüm bilgisini, "Sistem Veritabanı" kartı durumu (tıklayınca Veritabanı bölümüne gider), "Güncelleme" kartı son kontrol bilgisini gösterir. Altta "Kullanıcıya Ait Firmalar" listesi, "Yeni Firma" butonu, her firmada "Gelişmiş Yönetim" bağlantısı ve dönem satırında yeşil "Açık" rozeti vardır.
Etiket: giriş bölümü, firmalar, yeni firma, gelişmiş yönetim

## Görünüm ayarları
"Tema" açılır listesinden "Sistemi kullan / Açık / Koyu" seçilir. "Açılış adımı beklemesi" (ms) açılışta animasyon beklemesini, "Durum mesajı süresi" (ms) mesajların ekranda kalma süresini belirler. "Bildirimler" anahtarı uygulama bildirimlerini açar/kapatır.
Etiket: tema, açık tema, koyu tema, bildirimler, durum mesajı

## Güvenlik ayarları
"Giriş Koruması" grubunda "Yanlış deneme sınırı", "Kilit süresi" (dk) ve "Deneme penceresi" (dk) ayarlanır. "Parola" grubunda "Parola karma gücü" ve "En az parola uzunluğu" belirlenir. Yönetici rozetli alanları yalnız yönetici değiştirebilir.
Etiket: güvenlik, yanlış deneme, kilit süresi, parola, karma gücü

## Firma ayarları
"Firma Kodu" grubunda "Firma kodu biçimi" ve "Sıkı doğrulama" anahtarı; "Yeni Dönem" grubunda yeni açılan dönemin varsayılan durumu; "Liste Görünümü" grubunda açık ve arşiv dönem listelerinin sayfa boyutu ayarlanır.
Etiket: firma kodu biçimi, sıkı doğrulama, yeni dönem durumu, sayfa boyutu

## Veritabanı ayarları
"Sistem Veritabanı" bölümünde "Sistem yedeği" sayısı, "Kilit bekleme süresi", "Journal modu", "Disk senkronizasyonu" ve "Yedek öncesi VACUUM" ayarlanır. "Mali Dönem Veritabanları" bölümünde "Manuel yedek" sayısı, "Otomatik temizleme", "Kapanışta otomatik yedek" ve "Otomatik yedek aralığı" (gün) belirlenir.
Etiket: sistem yedeği, journal modu, manuel yedek, otomatik temizleme, otomatik yedek

## Mali Dönem ayarları
"Liste Görünümü" (yedekler ve bilinmeyen yedekler sayfa boyutu), "Güncelleme" (toplu tarama boyutu, güncelleme denemesi) ve "Bağlantı" (bakım zaman aşımı, komut zaman aşımı, kilit bekleme süresi, bağlantı havuzu) ayarları buradadır.
Etiket: dönem listesi, toplu tarama, zaman aşımı, bağlantı havuzu

## Güncelleme ayarları
"Şimdi Kontrol Et" güncellemeyi denetler. "Güncelleme" sekmesinde sürüm/durum, ilerleme ve varsa güncelleme butonu görünür. "Ayarlar" sekmesinde "Güncelleme kaynağı" (adres + "Bağlantıyı Dene" + "Varsayılana sıfırla") ve "Seçenekler" ("Açılışta otomatik kontrol", "Bildirimleri göster", "Beta sürümleri dahil et") bulunur.
Etiket: güncelleme kontrolü, güncelleme kaynağı, beta sürüm

## Yapay Zeka ayarları
"Durum" grubunda "Sürüm hakkı", "Model" (ürün tarafından sabitlenmiştir), "Model durumu" ve "Yardım dizini" görünür. "Model yönetimi" grubunda indirilmiş modeller listelenir; "Yenile" listeyi tazeler, "Sil" modeli diskten kaldırır. "Davranış" grubunda "Asistan etkin", "Geçmiş turu" (0-20), "Madde sayısı" (1-12) ve "Soru zaman aşımı (sn)" ayarlanır.
Etiket: yapay zeka, model durumu, sürüm hakkı, asistan etkin, geçmiş turu

## Geliştirici Araçları bölümü
Yalnızca geliştirme (DEBUG) derlemesinde görünür; sarı uyarı bandıyla belirtilir. Modül entegrasyon testleri ("Tümünü test et"), Kurulum Kimliği ("Onar"/"Tara"/"Sıfırla"), Dönem Şema Damgaları, Güncelleme Kaynağı doğrulama, "AI Bağlantı Öz-testi", Tanılama ve "Günlük ve Klasörler" (ayrıntılı log, klasör açma) araçlarını içerir.
Etiket: geliştirici araçları, modül testi, kurulum kimliği, öz-testi, tanılama
