# Randevu Sistemi (Appointment System)

Blazor WebAssembly ve Minimal API kullanılarak geliştirilmiş bir randevu talep, listeleme ve yönetici onay/red akışı uygulaması.

## 📋 Proje Hakkında

Bu proje, kullanıcıların randevu talebi oluşturması ve yöneticilerin bu talepleri onaylaması/reddetmesi için geliştirilmiş bir web uygulamasıdır.

### Özellikler

- ✅ Randevu talep formu (Şube seçimi, tarih/saat, açıklama)
- ✅ Randevu listeleme (Filtreleme, arama, sıralama, sayfalama)
- ✅ Yönetici paneli (Bekleyen talepleri onaylama/reddetme)
- ✅ Detay modalı (Randevu detayları ve audit trail)
- ✅ FluentValidation ile doğrulama
- ✅ Audit trail (Durum değişiklik geçmişi)
- ✅ MudBlazor ile modern UI

## 🏗️ Mimari

Proje Clean Architecture prensiplerine uygun olarak katmanlı mimari ile geliştirilmiştir:

```
AppointmentSystem/
├── AppointmentSystem.Web/          # Blazor WebAssembly (UI)
├── AppointmentSystem.Api/           # Minimal API (Backend)
├── AppointmentSystem.Application/   # İş mantığı, DTOs, Servisler, Validators
├── AppointmentSystem.Domain/       # Entity'ler, Enum'lar
└── AppointmentSystem.Infrastructure/ # EF Core, DbContext, Migrations
```

### Katmanlar

- **Web**: Blazor WebAssembly UI katmanı
- **API**: Minimal API ile RESTful endpoint'ler
- **Application**: İş mantığı, servisler, DTOs, FluentValidation
- **Domain**: Entity'ler (Appointment, Branch, AppointmentAudit), Enum'lar
- **Infrastructure**: EF Core, DbContext, Migrations, Seed data

## 🛠️ Teknolojiler

- **.NET 8.0**
- **Blazor WebAssembly**
- **Minimal API**
- **Entity Framework Core**
- **PostgreSQL** (veya MSSQL)
- **MudBlazor** (UI Framework)
- **FluentValidation**

## 📦 Kurulum

### Gereksinimler

- .NET 8.0 SDK
- PostgreSQL (veya SQL Server)
- Visual Studio 2022 veya VS Code

### Adımlar

1. **Repository'yi klonlayın:**
```bash
git clone <repository-url>
cd appointment-system
```

2. **Veritabanı bağlantı string'ini yapılandırın:**

`src/AppointmentSystem.Api/appsettings.json` dosyasında PostgreSQL bağlantı string'ini düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=appointmentdb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

3. **PostgreSQL veritabanını oluşturun:**
```sql
CREATE DATABASE appointmentdb;
```

4. **API projesini çalıştırın:**
```bash
cd src/AppointmentSystem.Api
dotnet run
```

API çalıştığında otomatik olarak:
- Migration'lar uygulanır
- Seed data oluşturulur (5 şube ve örnek randevu)

5. **WebAssembly projesini çalıştırın:**
```bash
cd src/AppointmentSystem.Web
dotnet run
```

6. **Tarayıcıda açın:**
- Web: `https://localhost:7000` veya `http://localhost:5000`
- API Swagger: `https://localhost:7236/swagger`

## 🔐 Login Bilgileri

**Not:** Bu proje şu anda authentication içermemektedir. Kullanıcı adları hardcoded olarak kullanılmaktadır:

- **Kullanıcı**: "Kullanıcı" (randevu oluştururken)
- **Yönetici**: "Admin" (onay/red işlemlerinde)

Gerçek bir uygulamada ASP.NET Core Identity veya JWT Authentication kullanılmalıdır.

## 📊 Seed Verisi

Uygulama ilk çalıştırıldığında otomatik olarak:

- **5 Şube** oluşturulur:
  - İstanbul Şube
  - Ankara Şube
  - İzmir Şube
  - Bursa Şube
  - Antalya Şube

- **1 Örnek Randevu** oluşturulur (Pending durumunda)

## 🎯 Kullanım Senaryoları

### Kullanıcı (Müşteri/Personel)

1. **Randevu Talep Formu** sayfasına gidin
2. Şube seçin (dropdown'dan 5 şube arasından)
3. Tarih ve saat bilgilerini girin
4. Açıklama ekleyin (opsiyonel)
5. "Gönder" butonuna tıklayın
6. Randevu **Pending** durumuna geçer

### Yönetici

1. **Yönetici Paneli** sayfasına gidin
2. Bekleyen (Pending) randevu taleplerini görüntüleyin
3. Her randevu için:
   - **Onayla**: Randevuyu onaylar (Approved)
   - **Reddet**: Red nedeni girerek reddeder (Rejected - açıklama zorunlu)

### Randevu Listesi

- Tüm randevuları görüntüleyin
- Durum, tarih aralığı ve arama ile filtreleyin
- Tarih veya duruma göre sıralayın
- Sayfalama ile gezinin (10/25/50 kayıt)

## 📝 API Endpoints

### Branches
- `GET /api/branches` - Tüm şubeleri listele
- `GET /api/branches/{id}` - Şube detayı

### Appointments
- `GET /api/appointments` - Randevuları listele (filtreleme, sayfalama)
- `GET /api/appointments/pending` - Bekleyen randevular
- `GET /api/appointments/{id}` - Randevu detayı
- `GET /api/appointments/{id}/audits` - Randevu audit geçmişi
- `POST /api/appointments` - Yeni randevu oluştur
- `PUT /api/appointments/{id}` - Randevu güncelle
- `POST /api/appointments/{id}/approve` - Randevu onayla
- `POST /api/appointments/{id}/reject` - Randevu reddet

## ✅ Doğrulama Kuralları

- **Şube**: Zorunlu
- **Talep Tarihi**: Bugünden önce olamaz
- **Başlangıç Saati**: Zorunlu
- **Bitiş Saati**: Başlangıç saatinden sonra olmalı
- **Red Nedeni**: Reddetme işleminde zorunlu

## 🔄 Durum Akışı

```
Draft → Pending → Approved
                ↘ Rejected
```

- **Draft**: Yeni kayıt oluşturulurken
- **Pending**: Kullanıcı talebi gönderdiğinde
- **Approved**: Yönetici onayladığında
- **Rejected**: Yönetici reddettiğinde (açıklama zorunlu)

## 🐛 Hata Yönetimi

- FluentValidation ile alan bazlı doğrulama
- Toast/Snackbar bildirimleri (başarılı/hatalı işlemler)
- Global hata yönetimi

## 📸 Ekran Görüntüleri

1. **Randevu Listesi**: Filtreleme, arama, sıralama ve sayfalama
2. **Randevu Talep Formu**: Şube seçimi, tarih/saat, validasyon
3. **Yönetici Paneli**: Onay/Red akışı

## 🚀 Geliştirme

### Migration Oluşturma

```bash
cd src/AppointmentSystem.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../AppointmentSystem.Api
```

### Migration Uygulama

```bash
cd src/AppointmentSystem.Api
dotnet ef database update
```

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👨‍💻 Geliştirici Notları

- Proje .NET 8.0 ile geliştirilmiştir
- PostgreSQL veritabanı kullanılmıştır (MSSQL'e değiştirilebilir)
- MudBlazor 8.14.0 kullanılmıştır
- FluentValidation 11.10.0 kullanılmıştır

## 🔮 Gelecek Geliştirmeler

- [ ] Authentication & Authorization (JWT/Identity)
- [ ] Email bildirimleri
- [ ] Randevu çakışma kontrolü
- [ ] Takvim görünümü
- [ ] Export/Import özellikleri
- [ ] Raporlama

---

**Not:** Bu proje bir örnek uygulamadır ve production için ek güvenlik önlemleri alınmalıdır.
