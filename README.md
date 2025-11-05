Bu proje katmanlı mimarideki yazılım hatalarını hata ayıklama çalışmasıdır. Projede, build (derleme), runtime (çalışma zamanı), mantıksal, async/await, N+1 query, mimari (architecture) ve memory leak hataları düzeltilmiştir.

Açıklama satırlarında belirtilmiş tüm hataların nasıl düzeltildiği satır satır açıklanmıştır. Gereksiz veya yanlış tanımlamalar doğrudan silinmemiş, düzletilen hataların nasıl düzeltildiği incelenebilmesi amacıyla sadece açıklama satırına alınmıştır. Aynı zamanda yapılan düzeltmeler adım adım commit edilmiştir.

 .Result, .Wait() ve GetAwaiter().GetResult() gibi async/await anti-pattern kullanımları kaldırılmış, tüm işlemler asenkron hale getirilmiştir. N+1 sorgu problemleri için Include() ve ThenInclude() ifadeleri eklenerek tek sorguda veri çekimi gerçekleştirilmiştir. Controller katmanında doğrudan veritabanı erişimine neden olan mimari ihlaller kaldırılmış, servis katmanı üzerinden erişim sağlanmıştır. 
