# Geliştirici Araçları

## Bu bölüm nedir?
Yalnızca geliştirme (DEBUG) derlemesinde görünen iç araçlardır; normal kullanıcı akışının parçası değildir. Her aksiyon onay ister ve Sistem günlüğüne DEV kaynağıyla yazılır.
Etiket: geliştirici araçları, debug, iç araç

## Kimlik durumu ve damga ne demek?
Kurulum kimliği bu uygulamanın kurulumunu, makine kimliği ise cihazı tanımlar. Listede her dönem veritabanının damgası (şema sürümü + kimlik) ve güncel kimlikle eşleşip eşleşmediği görünür. "Kimliği Onar" makinesi aynı olan dönemlerin damgasını eşitler; "Kimliği Sıfırla" yıkıcıdır ve yalnız kontrollü senaryoda kullanılır.
Etiket: kimlik, damga, onar, sıfırla

## Transfer taraması ne yapar?
Açılışta çalışan taşınmış-veri taramasını elle tetikler; sonuçta farklı kuruluma ait dönem sayısını veya sessiz onarılan kimlik sayısını gösterir.
Etiket: transfer taraması, taşınmış veri

## AI bağlantı öz-testi
"Öz-testi Çalıştır" sürüm hakkını, model durumunu ve yardım derlemini (sayfa/madde sayısı) tek listede gösterir. Salt-okunurdur, model indirmez.
Etiket: ai öz-testi, tanılama, model durumu

## Tanılama ve modül entegrasyon testleri
Tanılama, sistem testlerini (dosya/bağlantı/migration/veri/yetki) ve güncelleme kaynağı öz-testini tek listede PASS/FAIL olarak gösterir. "Modül Entegrasyon Testleri" her modülün DI'da çözülebildiğini ve kritik akışının salt-okunur çalıştığını kontrol eder; hangi modülün koptuğunu gösterir.
Etiket: tanılama, modül testi, di, pass fail

## Log ve veri klasörleri
"Log klasörünü aç" ve "Veri klasörünü aç" düğmeleri ilgili yolları dosya gezgininde açar. Yollar salt-okunur gösterilir. "Ayrıntılı log" dosya günlüğünü Debug seviyesine indirir.
Etiket: log, veri klasörü, ayrıntılı log
