# Yapay Zeka Yardım Asistanı

## Asistan nedir, nasıl açarım?
Uygulama içi yardım asistanıdır ve tamamen yerel çalışır: veri makineden çıkmaz, kayıt yazmaz. Çalışma alanında durum çubuğunun en sağındaki "Asistan" düğmesine basıp panele soru yazarsınız. Yanıtlar uygulamanın yardım bilgi tabanına dayanır.
Etiket: yapay zeka, asistan, sohbet, yerel

## Model indirme nasıl olur?
Model ve yürütücü indirme çalışma alanındaki Asistan panelinden yapılır; ilk soruda otomatik indirilir ve ilerleme panelde görünür. Panelde model durumu "Model hazır" veya "ilk soruda indirilecek" şeklinde dürüstçe gösterilir.
Etiket: model indirme, ilk soru, yürütücü, model hazır

## Modeli ben değiştirebilir miyim?
Hayır. Sohbet modeli ürün tarafından sabitlenmiştir (varsayılan qwen2.5-0.5b) ve kullanıcı/yönetici değiştiremez. Denetim Masası > Yapay Zeka bölümündeki "Model" kartı yalnızca sabit modelin adını gösterir. Bu, asistanın davranışını tutarlı ve tekrarlanabilir tutmak içindir.
Etiket: sabit model, model değiştirme, ürün sabiti

## Daha önce indirilmiş modelleri nasıl yönetirim?
Yapay Zeka bölümündeki "Model yönetimi" listesinde indirilmiş modeller boyutu ve "Yüklü" rozetiyle görünür; "Yenile" listeyi ve disk kullanımını tazeler. "Sil" modeli diskten kalıcı olarak kaldırır: önce onay ister, yüklü modeli silmeden önce bellekten bırakır. Silme geri alınamaz.
Etiket: model yönetimi, model silme, disk, yüklü

## Asistan hangi sürümlerde çalışır?
Asistan Profesyonel ve Kurumsal sürümlerde çalışır; Deneme sürümünde açıktır. Hakkınız yoksa gerekçesi Denetim Masası > Yapay Zeka bölümündeki "Sürüm hakkı" kartında yazar ve sohbet paneli kilitli görünür.
Etiket: sürüm, lisans, deneme, kilit

## Asistan neden kilitli görünüyor?
Nedenleri: sürüm hakkı yok, kullanıcı yetkisi (AiAsistan_Kullan) yok ya da asistan ayarlardan kapatılmış. Kilit bandındaki mesaj nedeni söyler; yönetici asistanı Denetim Masası > Yapay Zeka bölümünden açabilir.
Etiket: kilit, yetki, etkin, kapalı

## Davranış eşikleri ne işe yarar?
"Geçmiş turu" (soruya eklenen konuşma, 0-20), "Madde sayısı" (yanıta giren yardım maddesi, 1-12) ve "Soru zaman aşımı (sn)" (10-300) Denetim Masası > Yapay Zeka bölümünden ayarlanır.
Etiket: ayar, geçmiş turu, madde sayısı, zaman aşımı

## "Yardım dizini" neyi gösterir?
Asistanın dayandığı yardım maddeleri uygulamaya gömülü Markdown içerikten gelir ve AsistanBilgi.db dizininde tutulur. Kart "N madde • anlamsal arama açık" veya "N madde • yalnız anahtar kelime" gösterir. İlk soruda kurulur; model indirilemezse yalnız anahtar kelimeyle arama yapılır ve asistan çalışmaya devam eder.
Etiket: yardım dizini, anlamsal arama, anahtar kelime, madde sayısı

## Asistan verilerime yazıyor mu?
Hayır. Asistan salt-okunurdur: yalnız yardım bilgi tabanına dayanarak cevap üretir, uygulamanın verisine kayıt yapmaz.
Etiket: salt-okunur, veri güvenliği, kayıt yazmaz
