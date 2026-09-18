# Güncelleme Sonrası Doğrulama

## Bu ekran ne zaman görünür?
Uygulama güncellendikten sonra otomatik açılır. Dört adımlı bir doğrulama çalışır ve sonuç özeti gösterilir; her adım bir rozetle (bekliyor, çalışıyor, başarılı, atlandı, uyarı, hata) takip edilir.
Etiket: güncelleme sonrası, doğrulama, adım şeridi

## Hangi adımlar çalışır?
Soldan sağa dört adım: "Uygulama Dosyaları" (kritik dosyalar ve sürüm), "Sistem Veritabanı" (şema uyumluluğu, güncelleme ve doğrulama), "Mali Dönem Veritabanları" (dönem taraması ve bozuk kurtarma) ve "AI Yardım Dizini" (asistan yardım dizini tazeleme; bloklamaz). Adım açıklamaları için rozetin üzerine gelin.
Etiket: uygulama dosyaları, sistem veritabanı, mali dönemler, yardım dizini

## Sonuç ne anlama gelir?
Özet satırı "Güncelleme sonrası doğrulama tamamlandı • X dönem tarandı • Y bozuk • Z güncelleme bekliyor" biçimindedir. Dikkat gerektiren bir durum varsa altında "Dikkat gerektiren durumlar:" başlığıyla listelenir.
Etiket: doğrulama özeti, dikkat, tarama sonucu

## Hangi butonlar çıkar?
Her şey temizse "Devam Et" (kısa süre sonra girişe otomatik geçilir). Dikkat gerektiren durum varsa "Devam Et" ile ilerlenir. Uygulama dosyaları güncellenemediyse "Kapat", sistem veritabanı güncellenemediyse "Veritabanı Yönetimi" butonu çıkar ve sizi sistem veritabanı ekranına yönlendirir.
Etiket: devam et, kapat, veritabanı yönetimi, sonuç butonu

## AI Yardım Dizini adımı neden "atlandı" olabilir?
Bu adım bloklamaz. Asistan için semantik indeks yoksa adım "atlandı" veya "uyarı" durumunda kalır; dizin, asistan panelinde ilk soru sorulduğunda tamamlanır. Yardım içeriği okunamazsa "Yardım dizini oluşturulamadı" mesajı görünür.
Etiket: yardım dizini, semantik indeks, atlandı, uyarı

## Doğrulama sırasında hata olursa?
Adım "hata" rozetiyle işaretlenir ve sonuç türüne göre başlık değişir: "Uygulama güncellenemedi" veya "Sistem veritabanı güncellenemedi". Doğrulama beklenmedik biçimde düşerse "Doğrulama tamamlanamadı" ve zaman aşımında "Doğrulama zaman aşımına uğradı" mesajı görünür.
Etiket: doğrulama hatası, zaman aşımı, güncelleme başarısız
