# Dönem Veritabanı Güncelleme (Şema Göçü)

## Dönem güncellemesi ne zaman ve nasıl yapılır?
Bir mali dönemin veritabanı şeması yeni sürümü gerektiriyorsa, o dönemi **seçtiğinizde (erişim anında)** onay penceresi çıkar. "Güncelle" derseniz göç **aynı ekranda** çalışır: önce güvenlik yedeği alınır, sonra göçler uygulanır, en son bağlantı ve şema doğrulanır. Ayrı bir güncelleme sayfası yoktur.
Etiket: şema güncelleme, göç, migration, dönem erişimi

## Güncelleme gerektiğini nasıl anlarım?
Dönem listesinde ilgili satırda "Güncelleme Gerekli" rozeti ve listenin üstünde sarı bilgi çubuğu görünür. Güncelleme, o dönem seçilip çalışma alanına geçilmek istendiğinde otomatik önerilir.
Etiket: güncelleme gerekli, rozet, bildirim

## Onay penceresi ne gösterir?
Firma, dönem yılı, veritabanı adı, mevcut ve hedef şema sürümü ile bekleyen göç sayısı ve "Ana değişiklik" bilgisi gösterilir. Seçenekler: "Güncelle", "Daha sonra" ve "Vazgeç". Daha sonra/Vazgeç derseniz güncelleme yapılmaz; başka bir dönem seçebilirsiniz.
Etiket: onay penceresi, güncelle, daha sonra, vazgeç

## Göç nasıl ilerler?
İşlem üç adımda yürür: **Yedek → Göç → Doğrulama**. İlerleme çubuğu sırayla dolar (Yedek ~%30, Göç ~%70, Doğrulama ~%100); üstünde o anki adım yazar ve çubuk geri sarmaz. Adımlar rozetlerle gösterilir.
Etiket: ilerleme, yedek, göç, doğrulama, adım

## Doğrulama başarısız olursa ne olur?
Doğrulama geçilemezse "Geri alma" adımı açılır ve güncelleme öncesi yedekten **otomatik** dönülür. Geri alma da başarısız olursa yedeğin yolu verilir; Mali Dönem Yönetimi → Dönem Yedekleri → Geri Yükle ile elle geri yükleyebilirsiniz.
Etiket: geri alma, rollback, hata, yedek

## Güncelleme bittiğinde ne olur?
Başarılı göç sonrası dönem seçili kalır ve tek bir sonuç bildirimi gösterilir. Ardından "Çalışma Alanına Geç" ile ana panele geçebilirsiniz.
Etiket: tamamlandı, çalışma alanı, devam et

## Uygulama güncellemesi ile dönem güncellemesi farkı nedir?
Uygulama güncellemesi (yeni sürüm) veri kökünü korur; güncelleme sonrası açılışta sistem veritabanı ve dönemler otomatik doğrulanır. Dönem şema göçü ise yalnız o dönemin veritabanını ilgilendirir ve erişim anında onayla yapılır.
Etiket: uygulama güncellemesi, veri güvenliği, doğrulama
