# Mali Dönem Yönetimi

## Bu pencere ne işe yarar?
Seçili firmanın mali dönemlerini yönetir: yedekleme, bakım, analiz, arşivleme ve silme. Soldan dönem seçin; sağdaki kartlar ve yedek listesi o döneme bağlanır. Pencere "Firma & Mali Dönem" ekranındaki "Mali Dönem İşlemleri" ile açılır.
Etiket: mali dönem yönetimi, yedekleme, bakım, arşivleme

## Sol listeden dönem seçimi
Açık dönemler listelenir; arşivli dönemler ayrı bölümde görünür. Bir döneme tıklamak sağ içeriği tamamen o döneme geçirir. Liste uzunsa sayfalama (ileri/geri) görünür.
Etiket: dönem listesi, arşiv, dönem seçimi, sayfalama

## Nasıl yedek alırım?
"Şimdi Yedekle" seçili dönemin yedeğini alır. "Toplu Yedekle" tüm dönemlere sırayla yedek alır; "Toplu Test" ve "Toplu Bakım" da tüm dönemlere uygulanır ve sonuç tek özetle bildirilir.
Etiket: yedek alma, toplu yedek, toplu bakım, toplu test

## Yedeği nasıl geri yüklerim?
Yedek listesi en yeniden eskiye sıralanır. "Geri Yükle" tek kapıdan geçer: yedek önce analiz edilir (bozuk dosya, sürüm uyumsuzluğu, kayıp kayıt) ve kayıp riski varsa 6 haneli tek-seferlik onay kodu istenir. Geri yükleme mevcut veriyi değiştirir. Yetkiniz yoksa işlem "Veritabanı Geri Yükleme" izni gerektiği gerekçesiyle engellenir.
Etiket: geri yükleme, restore, onay kodu, yedek analizi

## Yedek nasıl silerim?
Satırdaki "Sil" yedeği kaldırır. Silme sonrası yedek sayısı saklama alt sınırının altına düşecekse, dosya adını aynen yazmanız istenen yazılı onay penceresi çıkar. Yetkiniz yoksa "Veritabanı Silme" izni gerektiği gerekçesiyle engellenir.
Etiket: yedek silme, saklama alt sınırı, yazılı onay, veritabanı silme

## Bakım ve analiz nasıl yapılır?
"Bakım" VACUUM, REINDEX ve WAL işlemlerini seçili döneme uygular. "Derin Analiz Çalıştır" tablo bazlı sağlık ve anomali raporu üretir; sonuç ekranda listelenir.
Etiket: bakım, vacuum, reindex, wal, derin analiz

## Dönemi nasıl arşivlerim?
"Arşivle" (Kapat) dönemi kapatır: girişe kapanır, veri korunur. Arşiv bölümündeki "Arşivden Çıkar" ile dönem yeniden açılır. Bu işlemler "Mali Dönem Yönetimi" izni gerektirir.
Etiket: arşivleme, arşivden çıkarma, dönem kapatma

## Dönemi silersem ne olur?
Dönem silme kalıcıdır ve guard'lı onay ister: aktif dönem kilidi, başka kullanıcı bağlantısı ve yetki kontrol edilir; onay için dönemin yılını yazmanız istenir. "Dönemin yedeklerini de sil" kutusu ile yedeklerin korunup korunmayacağını seçersiniz. Emin değilseniz arşivlemeyi tercih edin.
Etiket: dönem silme, kalıcı silme, guard, yedekleri de sil

## Kayıt dışı (yetim) yedekler
Dönem kaydı olmayan yedekler "Kayıt Dışı Yedekler" penceresinde "Silinen Dönem" ve "Bilinmeyen" sekmelerinde sayaçlarıyla gösterilir. Bu pencereyi açıp en fazla silinebilir yedekleri görebilirsiniz.
Etiket: kayıt dışı yedek, yetim yedek, bilinmeyen yedek

## Ayarlar penceresi (Mali Dönem Yönetimi)
Pencere ayarları "Genel", "Panel görünümü" ve "Yedekleme" sekmelerinden yönetilir. Bu firma için liste sayfa boyutu ve yedek saklama sayısı buradan değiştirilir; aynı ayar Denetim Masası > Mali Dönem/Veritabanı bölümlerinde de bulunur.
Etiket: mali dönem ayarları, yedek saklama, sayfa boyutu, panel görünümü
