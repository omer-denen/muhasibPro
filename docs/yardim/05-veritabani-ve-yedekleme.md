# Veritabanı ve Yedekleme Ayarları

## Bu ekran ne yapar?
"Veritabanı Ayarları" yedek saklama ve otomatik temizleme davranışını yönetir. Kaydet butonu yoktur; yaptığınız değişiklikler otomatik kaydedilir ve anında etkili olur.
Etiket: veritabanı ayarları, yedek saklama, otomatik kayıt

## Manuel yedek saklama limiti nedir?
Her dönem için saklanacak en fazla manuel yedek sayısıdır (1-20, varsayılan 5). Limit aşılınca en eski yedek otomatik silinir (eskiden yeniye FIFO). Örnek: 5 seçiliyse 6. yedek alındığında 1. (en eski) yedek silinir.
Etiket: yedek limiti, saklama, fifo, varsayılan 5

## Otomatik silme anahtarı ne yapar?
"Limit aşılınca en eski yedek otomatik silinsin" açıkken limit aşımında temizlik otomatik yapılır. Kapalıysa limit aşılsa bile yedekler silinmez; temizliği elle yaparsınız.
Etiket: otomatik silme, yedek temizleme, limit

## Kapanışta otomatik yedek
"Kapanışta açık dönem + sistem yedeğini otomatik al" açıkken uygulama kapanırken açık dönemin ve Sistem.db'nin yedeği alınır. Uzun kapanış süresi istemiyorsanız kapalı tutabilirsiniz.
Etiket: kapanış yedeği, otomatik yedek, sistem yedeği

## Haftalık bütünlük kontrolü hatırlatması
"PRAGMA integrity_check 7 günden eski ise uyarı gösterilir." Bu hatırlatma açıkken, veritabanı bütünlük kontrolü 7 günü geçtiğinde uyarı çıkar; kontrolü erteleyebilirsiniz.
Etiket: bütünlük kontrolü, integrity_check, hatırlatma

## Yedek klasörü nerede?
Yedeklerin tutulduğu yol ekranda gösterilir (geliştirme derlemesinde Databases\Yedekler, normalde LocalAppData altındaki MuhasibPro\Yedekler). Dosyaları elle silmeyin; uygulama üzerinden yönetin.
Etiket: yedek klasörü, yedek dosyası, yol

## Bilgi kutusu ne anlatır?
Otomatik temizlemenin yalnızca manuel yedekler için uygulandığını, geçici/otomatik yedeklerin bu temizlikten etkilenmediğini ve limitin 1-20 aralığında olduğunu belirtir.
Etiket: yedek bilgisi, manuel yedek, geçici yedek

## Sistem veritabanı ile dönem veritabanı farkı nedir?
Sistem.db (sistem veritabanı) uygulamanın ortak verisini (kullanıcılar, firmalar, mali dönem kayıtları, ayarlar) tutar. Her mali dönemin ayrı bir veritabanı dosyası vardır. Sistem.db'ye özel ayarlar (sistem yedeği sayısı, journal modu, disk senkronizasyonu, yedek öncesi VACUUM) Denetim Masası > Veritabanı bölümündedir.
Etiket: sistem veritabanı, sistem.db, dönem veritabanı, journal modu
