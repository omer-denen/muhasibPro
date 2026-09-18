# Giriş ve Oturum

## Sisteme nasıl giriş yaparım?
"Kullanıcı Adı" ve "Şifre" alanlarını doldurup "Sisteme Giriş Yap" butonuna basın; Enter tuşu da girişi tetikler. Doğrulama sırasında buton "Doğrulanıyor..." olur ve bir halka döner. Bilgiler doğruysa "Firma Seçimi" adımına geçilir.
Etiket: giriş, oturum, kullanıcı adı, şifre, enter

## Ekranın üstündeki adım göstergesi ne demek?
Üstteki şerit akıştaki konumu gösterir: Kurulum → **Giriş** → Firma Seçimi → Çalışma Alanı. Bulunduğunuz adım vurgulanır; böylece nerede olduğunuzu ve sıradaki adımı görürsünüz.
Etiket: adım göstergesi, akış, giriş adımı

## Giriş yapamıyorum, ne yapmalıyım?
Alanlardan biri boşsa "Kullanıcı adı alanı boş geçilemez!" veya "Şifre alanı boş geçilemez!" uyarısı çıkar. Bilgiler hatalıysa satır içi kırmızı blokta "Kullanıcı adı veya şifre hatalı!" yazar. Şifrenizi bilmiyorsanız yöneticinizden sıfırlama isteyin.
Etiket: giriş hatası, boş alan, hatalı şifre

## Sistem Durumu kartı ne gösterir?
Sistem.db'nin hazır olup olmadığını gösterir: durum hapı "Hazır" veya "Kontrol" olur. Altında "Sistem.db • SQLite • {boyut} • WAL/N bekleyen" gibi ayrıntı yazar. Sistem veritabanı kurulmadıysa "Sistem Veritabanı — Kurulum Gerekli" görünür ve giriş kapalıdır.
Etiket: sistem veritabanı, durum, hazır, kurulum gerekli

## Teşhis bağlantısı ne yapar?
Karttaki "Teşhis" butonu Sistem Veritabanı Yönetimi ekranını açar; oradan sistemi analiz edebilir, onarabilir veya kurabilirsiniz.
Etiket: teşhis, sistem veritabanı yönetimi, analiz

## Şifreyi göster/gizle ve Beni hatırla
Şifre alanındaki göz simgesi şifreyi gösterip gizler. "Beni hatırla" işaretliyse başarılı girişten sonra hesap Hızlı Giriş listesine eklenir.
Etiket: şifre göster, beni hatırla, hızlı giriş

## Hızlı Giriş nasıl çalışır?
"Beni hatırla" ile kaydedilen hesaplar "HIZLI GİRİŞ" listesinde görünür. Bir satıra tıklamak kullanıcı adını ve şifreyi doldurur; satırdaki çarpı işareti hesabı listeden kaldırır. Liste boşsa "Henüz hatırlanan hesap yok. 'Beni hatırla' ile ekleyin." yazar.
Etiket: hızlı giriş, kayıtlı hesap, hesap kaldır

## İlk kez mi kullanıyorum?
Sistem veritabanı henüz kurulmadıysa girişten önce "İlk Kurulum" ekranı çıkar. Kurulum tamamlanınca giriş ekranına dönülür. (Bkz. Açılış ve Kurulum Ekranları.)
Etiket: ilk kullanım, kurulum, sistem veritabanı
