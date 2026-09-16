# Mali Dönem Yönetimi

## Bu pencere ne işe yarar?
Seçili firmanın mali dönemlerini yönetir: yedekleme, bakım, analiz, arşivleme ve silme. Soldan dönem seçin; sağdaki kartlar ve yedek listesi o döneme bağlanır.
Etiket: mali dönem yönetimi, yedekleme, bakım

## Sol listeden dönem seçimi
Açık dönemler listelenir; arşivli dönemler varsa ayrı bölümde görünür. Bir döneme tıklamak sağ içeriği tamamen o döneme geçirir.
Etiket: dönem listesi, arşiv, dönem seçimi

## Nasıl yedek alırım?
"Şimdi Yedekle" seçili dönemin yedeğini alır. "Toplu Yedekle", "Toplu Test" ve "Toplu Bakım" butonları tüm dönemlere sırayla uygulanır; sonuç özet bildirimle gelir.
Etiket: yedek alma, toplu yedek, toplu bakım

## Yedeği nasıl geri yüklerim?
Yedek listesi en yeniden eskiye sıralanır. "Geri Yükle" tek kapıdan geçer: yedek önce analiz edilir (bozuk dosya, yeni sürüm veya kayıp kayıt) ve kayıp varsa 6 haneli onay kodu istenir. Geri yükleme sonrası mevcut veri değişir.
Etiket: geri yükleme, restore, onay kodu, yedek analizi

## Bakım ve analiz nasıl yapılır?
VACUUM, REINDEX ve WAL işlemleri seçili döneme uygulanır. "Derin Analiz Çalıştır" tablo bazlı sağlık ve anomali raporu üretir.
Etiket: bakım, vacuum, reindex, wal, derin analiz

## Dönemi nasıl arşivlerim?
"Arşivle" dönemi kapatır: girişe kapanır, veri korunur. "Arşivden Çıkar" ile dönem yeniden açılır.
Etiket: arşivleme, arşivden çıkarma, dönem kapatma

## Dönemi silersem ne olur?
Silme kalıcıdır ve yazılı onay ister; yedek alınmadan yapılmaz. Emin değilseniz arşivlemeyi tercih edin.
Etiket: dönem silme, kalıcı silme, onay

## Liste sayfası ve yedek saklama ayarı
Bu firmaya özel liste sayfası boyutu ve yedek saklama sayısı Mali Dönem Yönetimi penceresindeki ayarlardan değiştirilir.
Etiket: ayarlar, yedek saklama, sayfa boyutu
