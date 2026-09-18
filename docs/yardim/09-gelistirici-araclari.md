# Geliştirici Araçları

## Bu bölüm nedir?
Yalnızca geliştirme (DEBUG) derlemesinde görünen iç araçlardır; normal kullanıcı akışının parçası değildir. Sarı uyarı bandı bunu belirtir. Her aksiyon onay ister ve Sistem günlüğüne DEV kaynağıyla yazılır.
Etiket: geliştirici araçları, debug, iç araç

## Modül Entegrasyon Testleri
"Tümünü test et" butonu her modülün DI'da çözülebildiğini ve kritik akışının salt-okunur çalıştığını kontrol eder; hangi modülün koptuğunu PASS/FAIL listesiyle gösterir.
Etiket: modül entegrasyon testleri, tümünü test et, di, pass fail

## Kurulum Kimliği ve Dönem Şema Damgaları
"Kurulum kimliği" bu uygulamanın kurulumunu, "Makine kimliği" cihazı tanımlar. "Onar" damgaları onarır, "Tara" transfer taramasını çalıştırır, "Sıfırla" yıkıcıdır ve yalnız kontrollü senaryoda kullanılır (onaylı). "Dönem Şema Damgaları" tablosunda her veritabanının şema/kimlik bilgisi listelenir.
Etiket: kurulum kimliği, makine kimliği, onar, tara, sıfırla, şema damgası

## Transfer taraması ne yapar?
Açılışta çalışan taşınmış-veri taramasını elle tetikler; sonuçta farklı kuruluma ait dönem sayısını veya sessiz onarım yapılan kimlik sayısını gösterir.
Etiket: transfer taraması, taşınmış veri

## Güncelleme Kaynağı doğrulama
Geliştirici bölümündeki güncelleme kaynağı alanında adres girilir; "Varsayılana sıfırla" ve "Kaynağı Doğrula" butonları kaynağın erişilebilirliğini denetler.
Etiket: güncelleme kaynağı, kaynağı doğrula, varsayılana sıfırla

## AI bağlantı öz-testi
"Öz-testi Çalıştır" sürüm hakkını, model durumunu ve yardım derlemini (sayfa/madde sayısı) tek listede gösterir. Salt-okunurdur, model indirmez.
Etiket: ai öz-testi, tanılama, model durumu, sürüm hakkı

## Tanılama (çalışma-zamanı)
"Çalıştır" sistem testlerini (dosya/bağlantı/migration/veri/yetki) ve güncelleme kaynağı öz-testini tek listede PASS/FAIL olarak gösterir.
Etiket: tanılama, çalışma zamanı testi, pass fail

## Günlük ve klasörler
"Log klasörünü aç" ve "Veri klasörünü aç" ilgili yolları dosya gezgininde açar; yollar salt-okunur gösterilir. "Ayrıntılı log" anahtarı dosya günlüğünü Debug seviyesine indirir.
Etiket: log, veri klasörü, ayrıntılı log, klasör aç
