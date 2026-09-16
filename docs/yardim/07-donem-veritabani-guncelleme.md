# Dönem Veritabanı Güncelleme (Şema Göçü)

## Dönem güncellemesi ne zaman ve nasıl yapılır?
Bir mali dönemin veritabanı şeması yeni sürümü gerektiriyorsa, o dönemi **seçtiğinizde** (erişim anında) onay dialogu çıkar. "Şimdi Güncelle" derseniz göç **aynı ekranda** çalışır: önce güvenlik yedeği alınır, sonra göçler uygulanır, en son bağlantı ve şema doğrulanır. Ayrı bir güncelleme sayfası yoktur.
Etiket: şema güncelleme, göç, migration, dönem erişimi

## Güncelleme gerektiğini nasıl anlarım?
Dönem listesinde ilgili satırda "Güncelleme Gerekli" rozeti ve listenin üstünde sarı bilgi çubuğu görünür. Güncelleme, o dönem seçilip çalışma alanına geçilmek istendiğinde otomatik olarak önerilir.
Etiket: güncelleme gerekli, rozet, bildirim

## Onay dialogu ne gösterir?
Firma, dönem yılı, veritabanı adı, mevcut ve hedef şema sürümü ile bekleyen göç sayısı gösterilir. Seçenekler: "Şimdi Güncelle", "Daha Sonra" ve "Vazgeç". Daha Sonra/Vazgeç derseniz güncelleme yapılmaz; başka bir dönem seçebilirsiniz.
Etiket: onay dialogu, şimdi güncelle, daha sonra, vazgeç

## Göç nasıl ilerler?
İşlem üç adımda yürür: **Yedek → Göç → Doğrulama**. İlerleme çubuğu Yedek (~%30), Göç (~%70) ve Doğrulama (~%100) sırasıyla dolar; üstünde o anki adım yazar. Çubuk geri sarmaz. Adımlar Yedek, Göç ve Doğrulama rozetleriyle gösterilir.
Etiket: ilerleme, yedek, göç, doğrulama, adım

## Doğrulama başarısız olursa ne olur?
Doğrulama geçilemezse "Geri alma" adımı açılır ve güncelleme öncesi yedekten **otomatik** dönülür. Geri alma da başarısız olursa yedeğin yolu verilir; Mali Dönem Yönetim → Dönem Yedekleri → Geri Yükle ile elle geri yükleyebilirsiniz.
Etiket: geri alma, rollback, hata, yedek

## Güncelleme bittiğinde ne olur?
Başarılı göç sonrası dönem seçili kalır; tek bir sonuç bildirimi gösterilir. Ardından "Çalışma Alanına Geç" ile ana panele geçebilirsiniz.
Etiket: tamamlandı, çalışma alanı, devam et

## Uygulama güncellemesi ile dönem güncellemesi farkı nedir?
Uygulama güncellemesi (yeni sürüm) veri kökünü korur; güncelleme sonrası açılışta sistem veritabanı ve dönemler otomatik doğrulanır. Dönem şema göçü ise yalnız o dönemin veritabanını ilgilendirir ve erişim anında onayla yapılır.
Etiket: uygulama güncellemesi, veri güvenliği, doğrulama
