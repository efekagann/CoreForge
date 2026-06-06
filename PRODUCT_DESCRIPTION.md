# CoreForge — Ürün Tanımı ve Kapsam Belgesi

## CoreForge Nedir?

CoreForge, **C# / .NET 9** ile yazılmış, üretime hazır bir **SaaS (Software as a Service) başlangıç şablonudur**. Sıfırdan bir SaaS uygulaması geliştirmek isteyen yazılım geliştirici ve ekiplerin, tekrar eden altyapı kodunu yazmak yerine doğrudan iş mantığına odaklanmasını sağlar.

Bir SaaS ürünü kurarken her geliştiricinin çözmesi gereken onlarca ortak problem vardır: kimlik doğrulama, çok kiracılı veri izolasyonu, ödeme altyapısı, abonelik yönetimi, denetim kaydı, e-posta gönderimi, oran sınırlama, yerelleştirme... CoreForge tüm bu altyapıyı **kurumsal kalitede, test edilmiş ve genişletilebilir** biçimde hazır sunar.

---

## Hangi Sorunu Çözüyor?

Bir SaaS uygulaması sıfırdan inşa etmek genellikle **3–6 aylık altyapı çalışması** gerektirir. Bu sürenin büyük bölümü iş değeri üretmeyen ama zorunlu olan bileşenlere harcanır:

| Sorun | CoreForge'un Çözümü |
|-------|----------------------|
| Her müşterinin verisini birbirinden izole etmek | Satır bazlı çok kiracılı mimari, otomatik EF Core filtresi |
| Güvenli giriş sistemi kurmak | JWT + Redis yenileme tokeni, ASP.NET Identity |
| Ödeme almak ve abonelik yönetmek | Stripe entegrasyonu + Mock provider |
| Kim ne zaman ne değiştirdi? | Otomatik denetim kaydı (Audit Log) |
| Silinen veriyi geri getirebilmek | Soft Delete — kayıtlar fiziksel silinmez |
| E-posta göndermek | SMTP / MailKit + HTML şablon motoru |
| API'yi kötüye kullanımdan korumak | 3 seviyeli oran sınırlama (Rate Limiting) |
| Türkçe / İngilizce destek | Tam uygulama geneli çoklu dil desteği |
| Uzun süren işleri arka planda çalıştırmak | Kanal tabanlı arka plan iş kuyruğu |

---

## Kimler İçin?

- **Bağımsız yazılım geliştiriciler (Indie developers):** Kendi SaaS ürününü hızlıca pazara çıkarmak isteyenler
- **.NET ekipleri:** Yeni bir kurumsal SaaS projesine sağlam bir temelle başlamak isteyenler
- **Yazılım danışmanları:** Müşteri projelerinde tekrar eden altyapı kodunu ortadan kaldırmak isteyenler
- **.NET öğrencileri ve geliştiriciler:** Clean Architecture, CQRS, DDD gibi kurumsal kalıpları gerçek bir projede incelemek isteyenler

---

## Özellikler

### 1. Çok Kiracılı Mimari (Multi-Tenancy)

Her müşteri (kiracı) kendi verisini görür, başkasının verisine erişemez. Altyapı tamamen otomatiktir — yeni bir varlık (entity) eklendiğinde `ITenantScopedEntity` arayüzü implement edilmesi yeterlidir, filtre otomatik devreye girer.

- **Yöntem:** Satır bazlı izolasyon (tek veritabanı, her kayıtta `TenantId`)
- **Mekanizma:** EF Core global sorgu filtresi (yansıma tabanlı otomatik uygulama)
- **Başlık:** `X-Tenant-Id` HTTP başlığı ile kiracı bağlamı belirlenir
- **Süper Yönetici İstisnası:** `TenantId` başlığı olmadan yapılan istekler tüm kiracıların verisine erişebilir (SuperAdmin rolü için)

### 2. Kimlik Doğrulama ve Yetkilendirme

- **ASP.NET Core Identity** tabanlı kullanıcı yönetimi
- **JWT (JSON Web Token):** 15 dakika geçerlilik süresi
- **Yenileme Tokeni:** Redis'te saklanan, 7 günlük yenileme tokeni
- **Roller:** `SuperAdmin`, `Admin`, `User`
- **Endpoint'ler:** Kayıt, giriş, çıkış, token yenileme

### 3. Ödeme ve Abonelik Yönetimi

İki çalışma modu vardır, `appsettings.json`'da tek satır değişiklikle geçiş yapılır:

**Mock Provider (Geliştirme/Test):**
- Gerçek ödeme olmadan anında abonelik oluşturur
- Stripe hesabı gerekmez
- Test ve demo ortamları için idealdir

**Stripe Provider (Üretim):**
- Stripe Checkout oturumu oluşturur, kullanıcıyı Stripe'a yönlendirir
- Webhook ile ödeme onayı geldiğinde abonelik ve işlem kaydı oluşturulur
- `checkout.session.completed` olayı işlenir

**Plan Yapısı:**

| Plan | Açıklama |
|------|----------|
| Free | Ücretsiz, sınırlı özellikler |
| Starter | Temel SaaS özellikleri |
| Professional | Gelişmiş özellikler, daha yüksek limitler |
| Enterprise | Tam özellik seti, sınırsız kullanım |

Her plan için özellik anahtarları ve limit değerleri `IFeatureService` üzerinden sorgulanır. Yeni özellik veya limit eklemek için yalnızca `Features.cs` sabitleri genişletilir.

### 4. Denetim Kaydı (Audit Log)

Tüm `BaseEntity` türevleri için değişiklikler otomatik olarak kaydedilir. Kayıt şunları içerir:

- Hangi varlık türü değiştirildi (EntityName)
- Hangi kayıt (EntityId)
- İşlem türü: Oluşturuldu / Güncellendi / Silindi
- Değişiklikten önceki değerler (JSON)
- Değişiklikten sonraki değerler (JSON)
- Kim değiştirdi (UserId)
- Hangi kiracıya ait (TenantId)
- Saat

Herhangi bir kod yazılmasına gerek yoktur — `SaveChangesAsync` çağrıldığı anda tüm değişiklikler otomatik yakalanır.

### 5. Yumuşak Silme (Soft Delete)

`ISoftDeletable` arayüzünü implement eden varlıklar fiziksel olarak silinmez. Bunun yerine `DeletedAt` alanına silme zamanı yazılır ve EF Core global filtresi bu kayıtları sorgulardan otomatik olarak dışlar.

- Yanlışlıkla silinen veriler kurtarılabilir
- Veri geçmişi ve denetim izleri bozulmaz
- Kullanım: Entity'ye `ISoftDeletable` ekle — geri kalan her şey otomatik

### 6. E-posta Servisi

İki sağlayıcı desteklenir, `appsettings.json`'da geçiş yapılır:

- **Mock:** E-posta içeriğini loglara yazar, SMTP sunucusu gerektirmez
- **MailKit:** Gerçek SMTP sunucusu üzerinden gönderim

**HTML Şablon Motoru:**
- `Email/Templates/` klasöründe `.html` dosyaları
- `{{değişken}}` sözdizimi ile dinamik içerik
- Hazır şablonlar: `welcome.html`, `reset-password.html`

### 7. Oran Sınırlama (Rate Limiting)

API'yi kaba kuvvet saldırılarından ve aşırı kullanımdan korur:

| Politika | Limit | Kapsam | Amaç |
|----------|-------|--------|------|
| Default | 60 istek/dakika | IP adresi | Genel API koruması |
| Tenant | 300 istek/dakika | X-Tenant-Id | Kiracı bazlı kota |
| Auth | 10 istek/dakika | IP adresi | Brute-force koruması |

### 8. Arka Plan İş Kuyruğu

`IBackgroundJobQueue.QueueAsync()` ile uzun süren işler arka planda çalıştırılır:

```csharp
await jobQueue.QueueAsync(async ct =>
{
    await emailService.SendAsync("user@example.com", "Hoş Geldiniz", body);
});
```

- `System.Threading.Channels` tabanlı, harici kuyruklama sistemi (RabbitMQ vb.) gerektirmez
- `BackgroundJobProcessor` — `IHostedService` olarak kayıtlı, uygulama ömrü boyunca çalışır
- Kapasite: 100 iş (yapılandırılabilir)

### 9. Depolama Servisi

İki sağlayıcı, yapılandırmayla geçiş:

- **Local:** Dosyaları disk üzerine kaydeder (`uploads/` klasörü)
- **Mock:** Dosyaları bellek içinde tutar (test için)

`IStorageService` arayüzü üzerinden yükleme, indirme, silme ve genel URL alma işlemleri yapılır. Yeni sağlayıcı (AWS S3, Azure Blob vb.) eklemek için yalnızca arayüz implement edilir ve DI kaydı yapılır.

### 10. Çoklu Dil Desteği (Localization)

Tüm kullanıcıya dönük mesajlar (validasyon hataları, iş hataları, API yanıtları) çevirilebilir:

- **Desteklenen diller:** İngilizce (varsayılan), Türkçe
- **Mekanizma:** `Accept-Language` HTTP başlığı
- **Tip güvenli anahtarlar:** `ResourceKeys.Validation.FieldRequired` gibi sabitler
- **Kolay genişletme:** `.resx` dosyasına yeni satır + `ResourceKeys`'e sabit ekle

### 11. Gözlemlenebilirlik

- **Serilog:** Konsol + günlük dönen dosya loglaması
- **Sağlık Kontrolü:** `/health` endpoint'i (veritabanı bağlantısı dahil)
- **Swagger UI:** Tüm endpoint'ler için interaktif dokümantasyon, JWT Bearer desteği
- **İstek Loglaması:** Her HTTP isteği için otomatik süre ve durum kodu loglaması

---

## Mimari

CoreForge, **Clean Architecture** (Temiz Mimari) prensiplerine dayanır. Bağımlılık yönü her zaman içe doğrudur:

```
WebAPI → Identity / Infrastructure → Application → Domain
```

| Katman | İçerik |
|--------|--------|
| **Domain** | Varlıklar, arayüzler, değer nesneleri, iş kuralları |
| **Application** | CQRS komutları/sorguları, validatörler, DTO'lar, servis arayüzleri |
| **Infrastructure** | EF Core, ödeme, e-posta, depolama, arka plan işleri |
| **Identity** | ASP.NET Identity, JWT, kiracı sağlayıcısı |
| **WebAPI** | Controller'lar, middleware, Program.cs |

**Kullanılan Tasarım Kalıpları:**
- **CQRS** (MediatR) — Komut ve sorgu işlemleri ayrı handler'larda
- **Repository + Unit of Work** — Veri erişim soyutlaması
- **Pipeline Davranışları** — Tüm komutlar için otomatik validasyon
- **Provider Pattern** — Ödeme, e-posta, depolama sağlayıcıları çalışma zamanında değiştirilebilir
- **Global Sorgu Filtreleri** — Kiracı ve soft delete filtreleri otomatik uygulanır

---

## Teknoloji Yığını

| Kategori | Teknoloji | Versiyon |
|----------|-----------|---------|
| Framework | .NET / ASP.NET Core | 9.0 |
| ORM | Entity Framework Core | 9.x |
| Veritabanı | PostgreSQL | 17 |
| Cache / Token | Redis | 7 |
| Kimlik | ASP.NET Core Identity | 9.x |
| Mesajlaşma | MediatR | 14.x |
| Validasyon | FluentValidation | 12.x |
| Mapping | AutoMapper | 16.x |
| Loglama | Serilog | 10.x |
| Ödeme | Stripe.net | 47.x |
| E-posta | MailKit | 4.x |
| Dokümantasyon | Swashbuckle (Swagger) | 6.x |
| Test | xUnit + NSubstitute | 2.9 / 5.x |
| Container | Docker Compose | - |

---

## API Uç Noktaları

### Kimlik Doğrulama
| Method | Yol | Açıklama |
|--------|-----|----------|
| POST | `/api/auth/register` | Yeni kullanıcı kaydı |
| POST | `/api/auth/login` | Giriş → JWT + yenileme tokeni |
| POST | `/api/auth/refresh` | Token yenileme |
| POST | `/api/auth/logout` | Çıkış, yenileme tokenini iptal et |

### Kiracı Yönetimi (SuperAdmin)
| Method | Yol | Açıklama |
|--------|-----|----------|
| GET | `/api/tenants` | Tüm kiracıları listele |
| GET | `/api/tenants/{id}` | Kiracı detayı |
| POST | `/api/tenants` | Yeni kiracı oluştur |
| PUT | `/api/tenants/{id}` | Kiracıyı güncelle |
| DELETE | `/api/tenants/{id}` | Kiracıyı sil (soft delete) |

### Ödemeler
| Method | Yol | Açıklama |
|--------|-----|----------|
| POST | `/api/payments/checkout` | Ödeme oturumu başlat |
| POST | `/api/payments/webhook` | Stripe webhook (anonim) |
| GET | `/api/payments/history` | Ödeme geçmişi |
| GET | `/api/payments/subscription` | Aktif abonelik |

### Denetim Kaydı (SuperAdmin)
| Method | Yol | Açıklama |
|--------|-----|----------|
| GET | `/api/auditlog` | Denetim kayıtlarını filtreli listele |

### Sistem
| Method | Yol | Açıklama |
|--------|-----|----------|
| GET | `/health` | Sağlık kontrolü |
| GET | `/swagger` | API dokümantasyonu (geliştirme ortamı) |

---

## Hızlı Başlangıç

```bash
# Windows
.\setup.ps1 --seed

# Linux / macOS
chmod +x setup.sh && ./setup.sh --seed

# API'yi başlat
cd src/CoreForge.WebAPI && dotnet run
```

Swagger UI: `https://localhost:7001/swagger`

**Hazır gelen test hesapları:**

| Rol | E-posta | Şifre |
|-----|---------|-------|
| SuperAdmin | admin@coreforge.com | Admin@1234! |
| Tenant Admin | admin@acme.com | Test@1234! |
| Tenant User | user@acme.com | Test@1234! |

---

## Yapılandırma

Tüm sağlayıcı değişimleri `appsettings.json`'da tek satır:

```json
{
  "DatabaseProvider": "PostgreSQL",   // "PostgreSQL" | "SqlServer"
  "PaymentProvider": "Mock",          // "Mock" | "Stripe"
  "Email": { "Provider": "Mock" },    // "Mock" | "MailKit"
  "Storage": { "Provider": "Local" }  // "Local" | "Mock"
}
```

---

## Genişletme

### Yeni Varlık Eklemek
1. `Domain/Entities/` altında class oluştur, ihtiyaca göre `ITenantScopedEntity` ve `ISoftDeletable` ekle
2. `Infrastructure/Persistence/Configurations/` altında EF konfigürasyonu yaz
3. `ApplicationDbContext`'e `DbSet` ekle
4. Migration oluştur: `dotnet ef migrations add YeniVarlık`
5. `Application/Features/` altında CQRS handler'larını yaz
6. `WebAPI/Controllers/` altında controller ekle

Kiracı filtresi, soft delete ve audit log otomatik devreye girer.

### Yeni Ödeme Sağlayıcısı Eklemek
1. `Infrastructure/Payments/` altında `IPaymentService` implement et
2. `DependencyInjection.cs`'de yeni case ekle
3. `appsettings.json`'da sağlayıcı adını yaz

---

## Test

```bash
# Tüm unit testleri çalıştır
dotnet test --filter "Category!=Integration"

# Kapsam raporu ile
dotnet test --collect:"XPlat Code Coverage"

# Integration testleri (Docker gerektirir)
docker compose up -d
dotnet test --filter "Category=Integration"
```

**Mevcut test kapsamı:**
- Domain katmanı: 11 test (BaseEntity, Result, Tenant)
- Application katmanı: 28 test (5 handler sınıfı, 4 ödeme senaryosu, validator)
- Integration: WebApplicationFactory şablonu hazır
