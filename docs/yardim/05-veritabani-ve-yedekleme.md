# Veritabanı ve Yedekleme Ayarları

## Manuel yedek saklama limiti nedir?
Her dönem için saklanacak en fazla manuel yedek sayısıdır (1-20). Limit aşılınca en eski yedek otomatik silinir (FIFO).
Etiket: yedek limiti, saklama, fifo

## Otomatik temizleme ne yapar?
Kapalıysa, limit aşılsa bile yedekler silinmez; temizliği elle yaparsınız. Açıkken limit aşımında en eski yedekler otomatik kaldırılır.
Etiket: otomatik temizleme, yedek silme

## Kapanışta otomatik yedek
Uygulama kapatılırken açık dönemin ve Sistem.db'nin yedeği alınır. Uzun kapanış süresi istemiyorsanız bu seçeneği kapalı tutun.
Etiket: kapanış yedeği, otomatik yedek

## Haftalık bütünlük hatırlatması
Veritabanı bütünlük kontrolü (PRAGMA integrity_check) 7 günden eskiyse uyarı gösterilir; kontrolü erteleyebilirsiniz.
Etiket: bütünlük kontrolü, integrity_check, hatırlatma

## Yedek klasörü nerede?
Tüm manuel yedekler yedek klasöründe saklanır. Dosyaları elle silmeyin; uygulama üzerinden yönetin.
Etiket: yedek klasörü, yedek dosyası

## Sistem veritabanı nedir?
Uygulamanın ortak verisini (kullanıcılar, firmalar, mali dönem kayıtları, ayarlar) tutan sistem.db'dir. Her mali dönemin ayrı bir veritabanı dosyası vardır.
Etiket: sistem veritabanı, sistem.db, dönem veritabanı
