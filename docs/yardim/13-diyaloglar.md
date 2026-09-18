# Onay ve Bilgi Pencereleri (Diyaloglar)

## Yeni Mali Dönem penceresi
"Yeni Mali Dönem Aç" başlığı ve "Firma: {ünvan}" bilgisiyle açılır; "Bu işlem yeni bir SQLite veritabanı tahsis edecek ve muhasebe şemasını migrate edecektir" açıklaması yer alır. "Mali Dönem Yılı" (NumberBox) ve "Dönem Açıklaması" girilir; "Saga Pipeline Başlat" ile başlatılır, "İptal" ile kapatılır. Enter da onaylar.
Etiket: yeni mali dönem, dönem yılı, saga pipeline, veritabanı tahsisi

## Saga Pipeline penceresi
Yeni dönem açılışının adımlarını "Adım N: {ad}" satırları, açıklamaları ve zaman damgalarıyla gösterir. "Saga hattı bekleniyor — başlatmak için devam edin." durumuyla açılır; "Saga Hattını Başlat" ile çalışır, "Vazgeç" ile kapatılır.
Etiket: saga pipeline, dönem açma adımları, ilerleme

## Veritabanı Güncelleme (göç onayı)
Bir dönemin şema güncellemesi gerektiğinde "Veritabanı Güncelleme Gerekli" başlığıyla açılır. Firma, veritabanı adı/yıl, sürüm şeridi (mevcut → hedef), göç sayısı ve "Ana değişiklik" bilgisi gösterilir. "Güncelle", "Daha sonra" veya "Vazgeç" seçenekleri sunulur.
Etiket: göç onayı, şema güncelleme, daha sonra, güncelle

## Yedekten Geri Yükle (dönem) penceresi
Yedek kimlik kartı (dosya adı, boyut, firma/dönem/rol/kurulum/makine/sürüm) ve bir hüküm bandı gösterilir. Hükümler: "Geri yüklenebilir", "Kimliksiz (eski) yedek", "Eski sürüm yedek", "Taşınmış yedek", "Kayıp kayıt riski", "Firma/mali dönem/veritabanı adı uyuşmazlığı", "Yedek dosya bozuk", "Sürüm uyumsuz — güncelleme gerekli". Kayıp riski varsa 6 haneli tek-seferlik onay kodu istenir ("6 haneli kodu girin"); hatalı kodda "Onay kodu hatalı." uyarısı çıkar. Bozuk yedekte "Geri Yükle" pasiftir. "Geri Yükle" / "Vazgeç".
Etiket: geri yükleme, yedek analizi, onay kodu, kayıp kayıt

## Sistem Veritabanını Geri Yükle penceresi
Sistem.db geri yüklemesinde açılır. Yedek kartı (boyut, tarih, dosyadaki sürüm) ve varsa "Geri yüklemede kaybolacak kayıtlar" kartı gösterilir. Gerekiyorsa "Tek-seferlik onay kodu" girilir. "Geri Yükle" / "Vazgeç".
Etiket: sistem geri yükleme, kayıp kayıtlar, onay kodu

## Mali Dönem Silme (guard) penceresi
"Dönemi Sil" öncesi guard kontrolleri gösterilir: aktif dönem kilidi, başka kullanıcı bağlantısı ve yetki ("Veritabani_Sil"). Onay için dönemin yılını yazmanız istenir ("Örn: 2027"); hatalıysa "Onay kodu hatalı." uyarısı çıkar. "Dönemin yedeklerini de sil" kutusu (varsayılan işaretli) ile yedeklerin korunup korunmayacağı seçilir. "Dönemi Sil" / "Vazgeç".
Etiket: dönem silme, guard, yıl onayı, yedekleri de sil

## Yedek Silme Onayı (alt sınır) penceresi
Silme sonrası yedek sayısı saklama alt sınırının altına düşecekse çıkar. "Saklama alt sınırının altına düşeceksiniz" uyarısıyla birlikte dosya adını aynen yazmanız istenir ("Dosya adını buraya yazın"); hatalıysa "Onay kodu hatalı." çıkar. "Kalıcı Sil" / "Vazgeç".
Etiket: yedek silme onayı, saklama alt sınırı, dosya adı onayı

## Taşınmış Veri bildirimi
Başka bir kurulum veya makinede oluşturulmuş dönem veritabanları algılanırsa açılır. Mevcut kurulum/makine ve etkilenen veritabanları listelenir. Bu bildirim yalnızca geliştirme derlemesinde gösterilir; "Kapat" ile kapatılır.
Etiket: taşınmış veri, kurulum kimliği, makine, bildirim

## Kayıt Dışı Yedekler penceresi
Dönem kaydı olmayan yedekleri "Silinen Dönem" ve "Bilinmeyen" sekmelerinde sayaçlarıyla gösterir. "Kapat" ile kapatılır.
Etiket: kayıt dışı yedek, yetim yedek, silinen dönem

## Veritabanı Onarımı penceresi
Sistem.db hasarlıysa açılır: "Onarım, veritabanını silip sıfırdan oluşturacak. Mevcut kullanıcı ve firma kayıtları kaybolabilir." "Onar ve Yeniden Oluştur" / "İptal".
Etiket: onarım, sistem veritabanı, yeniden oluştur

## Kurulum Kimliği işlemleri (geliştirici)
Geliştirici Araçları'nda "Kurulum Kimliğini Sıfırla" yeni bir kimlik üretip dönem damgalarını yeniler ("Sıfırla" / "Vazgeç"); "Kurulum Kimliğini Onar" damgaları onarır ("Onar" / "Vazgeç").
Etiket: kurulum kimliği, sıfırla, onar, geliştirici
