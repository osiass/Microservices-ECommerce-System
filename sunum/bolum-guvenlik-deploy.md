BÖLÜM 3 — GÜVENLİK VE DEPLOY İYİLEŞTİRMELERİ


Neden Yapıldı?

Deploy sırasında sipariş verilerinin kaybolması ve hassas bilgilerin kaynak kodda düz metin olarak bulunması iki kritik sorun olarak tespit edildi.


DEĞİŞTİRİLEN VE OLUŞTURULAN DOSYALAR


1. WebUI/Auth/NoOpAuthHandler.cs — Yeni dosya

ASP.NET Core'un authorization middleware'i, Blazor sayfalarındaki Authorize attribute'larını görünce HTTP seviyesinde /Account/Login'e yönlendirme yapıyordu. Bu sayfa mevcut olmadığından 404 hatası alınıyordu.

Çözüm olarak kimlik doğrulamayı hiçbir şey yapmadan geçiren bir handler yazıldı. Handler hiçbir zaman challenge atmaz, yönlendirme yapmaz. Tüm auth kararı Blazor'un kendi AuthorizeRouteView mekanizmasına bırakılır.


2. WebUI/Program.cs

Cookie authentication kaldırılarak NoOp scheme eklendi. DefaultAuthenticateScheme, DefaultChallengeScheme ve DefaultForbidScheme alanlarının üçü de NoOp olarak ayarlandı; aksi halde ASP.NET Core kısmen devreye girebilir ve beklenmedik yönlendirmeler oluşabilir.


3. deploy.ps1

Deploy sırasında sipariş verisi kaybolmasını önlemek için yedekleme eklendi. Deploy başlamadan önce pg_dump ile Orders ve OrderItems tablolarının veri yedeği alınır. Deploy tamamlanıp API'lar ayağa kalktıktan sonra yedek psql ile geri yüklenir.

Ek olarak bash syntax'ından (2>/dev/null) PowerShell uyumlu syntax'a (2>&1) geçildi.


4. ECommerce.AppHost/aspirate-output/secrets.yaml — Yeni dosya

JWT anahtarı, Mailtrap tokeni ve veritabanı bağlantı stringleri ConfigMap'ten alınarak Kubernetes Secret'a taşındı. Secret değerleri base64 encode edilmiş biçimde saklanır ve pod'lara environment variable olarak aktarılır. Hassas bilgiler artık kaynak kodda veya git geçmişinde görünmez.

Her servisin deployment.yaml dosyasına secretRef eklenerek Secret değerleri pod'a aktarıldı. ConfigMap dosyalarından Jwt__Key satırı silindi; bu değer artık Secret'tan geldiğinden ConfigMap'te bulunması gerekmez.
