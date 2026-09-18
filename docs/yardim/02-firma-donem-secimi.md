# Firma ve Mali Dönem Seçimi

## Bu ekran ne yapar?
"Firma & Mali Dönem" ekranı çalışma ortamınızı seçtirir: hangi firmanın hangi mali döneminde çalışacağınızı belirler ve veritabanı bağlantısını kurar. Üstte "Ayarlar" (Denetim Masası), "Firma Yönetimi" (yetkiniz varsa) ve kullanıcı menüsü bulunur.
Etiket: firma seçimi, mali dönem, çalışma ortamı

## Firma nasıl seçerim?
"FİRMA" bölümündeki firma seçiciye tıklayın; açılan listede "Firma ara (kodu, ünvanı, şehir)..." kutusuna yazarak arayabilirsiniz. Eşleşme yoksa "Firma bulunamadı" yazar. Bir firmaya tıkladığınızda firma seçilir ve bilgileri kartta görünür.
Etiket: firma, firma ara, firma kodu, ünvan

## Seçili firma kartında neler var?
Firma kodu, kısa ünvan, il, vergi no ve dönem sayısı (pill) görünür; altta Yetkili, İletişim, Vergi ve Adres satırları yer alır. Kartın altındaki "Mali Dönem İşlemleri" butonu o firmanın yönetim penceresini açar, "Düzenle" butonu firma bilgilerini düzenletir.
Etiket: firma kartı, mali dönem işlemleri, düzenle

## Yeni firma nasıl eklerim?
"FİRMA" başlığının sağındaki "Yeni Firma" butonu firma tanımlama ekranını açar; kayıt sonrası liste tazelenir. Hiç firma yoksa "Firma seçilmedi" boş durumu ve "Yeni Firma" yönlendirmesi görünür.
Etiket: yeni firma, firma tanımlama

## Mali dönem nasıl seçerim?
"Mali Dönemler" listesinden bir satıra tıklayın; seçilen satır genişleyip veritabanı adı/boyutu, "DB Durumu" hapı ve "Son Yedek" bilgisini gösterir. Firma seçili değilse "Bir firma seçin" yazar.
Etiket: mali dönem, dönem seçimi, db durumu, son yedek

## Dönem rozetleri ne anlama gelir?
Satırlarda "Açık"/"Kapalı" durumu, "Veritabanı dosyası yok" için "DB Yok", şema güncellemesi bekleyenler için "Güncelleme Gerekli" ve en son çalışılan dönem için "Son çalışılan" rozeti görünür.
Etiket: açık dönem, kapalı dönem, db yok, güncelleme gerekli, son çalışılan

## Yeni mali dönem nasıl açarım?
"Mali Dönemler" başlığındaki "Yeni Mali Dönem Aç" butonu yeni dönem penceresini açar; veritabanı oluşturma adımları sırayla gösterilir. Hiç dönem yoksa boş-durum kartındaki "Yeni mali dönem aç" bağlantısı da aynı pencereyi açar.
Etiket: yeni dönem, mali dönem açma, veritabanı oluşturma

## Bir dönemin yedeğini buradan alabilir miyim?
Liste satırında genişlediğinde görünen "Yedek" butonu o dönemin hızlı yedeğini alır. Yedekleri geri yükleme, silme ve listeleme için "Mali Dönem İşlemleri" penceresini kullanın.
Etiket: dönem yedeği, yedek al, hızlı yedek

## Güncelleme bildirimi ne demek?
Bir dönemin şeması güncelleme bekliyorsa listenin üstünde sarı InfoBar "Veritabanı güncellemesi hazır" başlığıyla çıkar. Tek dönemde eylem "Güncelle" doğrudan erişim anında onayı açar; birden çok dönemde "İncele" ile listeden dönem seçersiniz.
Etiket: şema güncellemesi, veritabanı güncellemesi hazır, incele

## "Çalışma Alanına Geç" neden pasif?
Firma ve açık bir dönem seçiliyken sağ alttaki "Çalışma Alanına Geç" ile ana panele geçilir. Buton pasifse nedenini üzerine gelerek görürsünüz: "Önce firma ve mali dönem seçin", "Veritabanı dosyası yok..." veya "Kapalı döneme girilemez...". Sol taraftaki seçim özeti de seçiminizi gösterir.
Etiket: çalışma alanı, geçiş, pasif buton, seçim özeti

## Ayarlar ve Firma Yönetimi
Sağ üstteki "Ayarlar" Denetim Masası'nı ayrı pencerede açar. "Firma Yönetimi" butonu firma listesi/detay yönetimini açar; bu buton yalnızca "Firma Yönetimi" izni olan kullanıcılarda etkindir, yetkiniz yoksa nedenini tooltip'te gösterir.
Etiket: ayarlar, denetim masası, firma yönetimi, yetki
