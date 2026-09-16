# Dönem Veritabanı Güncelleme (Şema Göçü)

## Bu sayfa ne yapar?
Seçili mali dönemin veritabanı şemasını yeni sürüme günceller. Akış üç adımdır: önce güvenlik yedeği alınır, sonra bekleyen göçler uygulanır, en son bağlantı ve şema doğrulanır.
Etiket: şema güncelleme, göç, migration, dönem veritabanı

## Durum kartı (sürüm şeridi) ne gösterir?
Solda mevcut, sağda güncellenecek şema sürümü görünür; yanındaki rozet bekleyen göç sayısını gösterir. Alt satırdaki tek hüküm, bu dönem için ne olacağını özetler.
Etiket: sürüm şeridi, bekleyen göç, durum

## İlerleme nasıl gösterilir?
İşlem çalışırken çubuk Yedek (~%30), Göç (~%70) ve Doğrulama (~%100) sırasıyla dolar; üstünde o anki adım yazar. Çubuk geri sarmaz.
Etiket: ilerleme, yedek, doğrulama

## Doğrulama başarısız olursa ne olur?
Doğrulama geçilemezse "Geri alma" adımı açılır ve yedekten otomatik dönülür. Geri alma da başarısız olursa yedeğin yolu verilir (elle geri yükleme için).
Etiket: geri alma, rollback, hata

## Hangi değişiklikler yapılacak?
Sayfa, hangi tabloya hangi kolonların eklendiğini veya güncellendiğini gösterir. "Kaldırılanlar" satırları yalnızca bilgilendirir; veri silinmez.
Etiket: değişiklikler, tablo, kolon

## Güncellemeyi nasıl başlatırım?
"Yedekle ve Güncelle" işlemi başlatır. Buton yalnız göç gerekiyorsa ve işlem çalışmıyorken aktiftir. Hata olursa kırmızı bantta neden yazar.
Etiket: yedekle ve güncelle, başlatma

## Güncelleme bitince ne olur?
Doğrulama başarılı olduğunda "Çalışma Alanına Geç" görünür. Bu buton seçimi kaydeder, pencereyi kapatır ve ana ekrandaki "Devam Et" ile çalışma alanına geçilir.
Etiket: tamamlandı, çalışma alanı, devam et
