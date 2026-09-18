# Sistem Veritabanı Yönetimi

## Bu ekran ne işe yarar?
Sistem veritabanını (Sistem.db) kurar, doğrular, onarır, yedekler ve günceller. Üst başlıkta canlı durum mesajı ve durum hapı (Güncel / Hasarlı / Kurulum Gerekli) görünür. Ekran dört bölümden oluşur: "Durum ve işlemler", "Yedekleme ve güncelleme", "Tanı ve kurulum kaydı" ve "İşlem günlüğü".
Etiket: sistem veritabanı yönetimi, kurulum, onarım, yedekleme

## Durum ve işlemler
"Sistem Durumu" kartı BOYUT / SÜRÜM / BİLEŞEN / DOĞRULAMA metriklerini ve veritabanı dosya yolunu gösterir. Altındaki "Veritabanı İşlemleri" kartında "Sistemi Analiz Et" butonu vardır. Veritabanı geçerliyse yeşil "Veritabanı Güncel" bandı, geçersizse kırmızı "Kurulum Gerekli" bandı ve "Onar" butonu görünür.
Etiket: sistem durumu, veritabanı işlemleri, sistemi analiz et

## Kurulum nasıl başlatılır?
Veritabanı bulunamadıysa "Kurulumu Başlat" butonu (birincil) yeni Sistem.db oluşturur. İlerleme yüzdeyle gösterilir; bitince durum "Veritabanı hazır" olur ve "Girişe geçebilirsiniz" mesajı görünür.
Etiket: kurulumu başlat, sistem veritabanı oluşturma, ilk kurulum

## Onarım nasıl yapılır?
Veritabanı hasarlıysa "Onar" butonu onay ister: "Mevcut Sistem.db dosyası hasarlı görünüyor. Onarım, veritabanını silip sıfırdan oluşturacak. Mevcut kullanıcı ve firma kayıtları kaybolabilir." Onaylarsanız sistem veritabanı silinip yeniden oluşturulur.
Etiket: onar, hasarlı veritabanı, yeniden oluştur

## Sistemi Analiz Et ne yapar?
Yedi test çalıştırır: dosya/konum, bağlantı, sistem veritabanı servisi, sistem verisi ve sistem yetkisi. Sonuçta "Test tamamlandı: X/Y başarılı" mesajı ve "Sistem Testleri" kartında her testin durumu (Başarılı/Başarısız) listelenir.
Etiket: analiz, sistem testleri, tanılama

## Yedek nasıl alınır?
"Şimdi Yedekle" Sistem.db yedeğini alır; ardından saklama sayısına göre eski yedekler temizlenir ("Yedek alındı, N eski yedek temizlendi" mesajı). "Yenile" listeyi tazeler. Liste boşsa "Henüz yedek yok — 'Şimdi Yedekle' ile ilk yedeği alın." yazar.
Etiket: sistem yedeği, şimdi yedekle, yedek temizleme

## Yedek nasıl geri yüklenir?
Listeden bir yedeğin "Geri Yükle" butonuyla tek-kapı akışı başlar: "Yedek analiz ediliyor..." → hüküm penceresi → "Geri yükleniyor...". Analiz; bozuk dosya, sürüm uyumsuzluğu, taşınmış/kimliksiz yedek ve kayıp kayıt riskini değerlendirir; kayıp riski varsa 6 haneli tek-seferlik onay kodu istenir. Geri yükleme mevcut veriyi değiştirir.
Etiket: yedekten geri yükleme, restore, onay kodu, yedek analizi

## Sistem güncellemesi nasıl uygulanır?
"Güncellemeyi Uygula" butonu bekleyen şema güncellemelerini uygular: önce manuel yedek alınır, göç uygulanır, sonuç doğrulanır. Adımlar "1/3 Yedek alınıyor...", "2/3 Göç uygulanıyor...", "3/3 Doğrulanıyor..." olarak yüzdeyle gösterilir. Bekleyen güncelleme yoksa "Sistem güncel" yazar.
Etiket: sistem güncellemesi, göç, şema güncelleme, doğrulama

## Kurulum Kaydı ne gösterir?
"Tanı ve kurulum kaydı" bölümünde "Kurulum Kaydı" kartı bu kurulumun kimliğini (KurulumId, MakineId, Oluşturma) gösterir; bu kimlik yedeklere damgalanır.
Etiket: kurulum kaydı, kurulum kimliği, makine kimliği

## Giriş Ekranına Devam Et
Kurulum tamamlandıysa alt bardaki "Giriş Ekranına Devam Et" butonu görünür. Durum tazelenir; veritabanı hazır ve geçerliyse giriş ekranına geçilir, değilse "Veritabanı hazır değil" uyarısı verilir.
Etiket: giriş ekranına devam, geçiş, kurulum tamamlandı

## İşlem günlüğü
"Canlı onarım ve tanı akışı" bölümünde doğrulama, onarım ve güncelleme adımları canlı olarak yazılır; sağ üstte yeşil "CANLI" rozeti görünür.
Etiket: işlem günlüğü, canlı akış, tanı
