# 🎓 Staj Evrak Doğrulama ve Yönetim Sistemi

![.NET 10.0](https://img.shields.io/badge/.NET-10.0-purple.svg)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-blue.svg)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-SQLite-green.svg)
![License](https://img.shields.io/badge/License-MIT-yellow.svg)

Kocaeli Üniversitesi Yazılım Mühendisliği Bölümü **Programlama Laboratuvarı III** dersi kapsamında geliştirilmiş; öğrencilerin staj başvuru evraklarını (Ek-16) dijital ortamda yüklemesini, kurallara göre otomatik denetlenmesini ve yöneticilerin (admin) onay süreçlerini yönetmesini sağlayan web tabanlı bir otomasyon sistemidir.

---

## 🚀 Temel Özellikler

### 👨‍🎓 Öğrenci Paneli (ROLE_STUDENT)
- **Kendi Hesabını Oluşturma:** Şifre ve e-posta ile güvenli kayıt olma.
- **Ek-16 Evrak Yükleme:** Word (.docx, .doc), PDF ve görsel formatlarında evrak yükleme.
- **Dinamik Takvim ve Gün Seçimi:** Staj başlangıç/bitiş tarihleri ve haftalık staj günlerini belirleme.
- **Otomatik İş Günü Hesabı:** Resmi ve dini tatillerin otomatik düşülerek fiili staj gününün hesaplanması.
- **Durum Takibi ve Raporlama:** Başvuru statüsü (`Yuklendi`, `Inceleniyor`, `Onaylandi`, `Hatali_Eksik`, `Tamamlandi`) ve hata/eksik bildirimlerini görüntüleme.

### 👨‍💼 Yönetici Paneli (ROLE_ADMIN)
- **Gelişmiş Filtreleme ve Arama:** Başvuruları duruma göre filtreleme ve öğrenci adı/no ile arama.
- **Evrak İnceleme ve Statü Güncelleme:** Başvuruyu inceleyip onaylama veya eksik olarak işaretleme.
- **Tamamlananlar Arşivi:** Onaylanıp tamamlanan başvuruların ayrı sekmede listelenmesi.
- **Test Veri Seti İçe Aktarma:** 12 sütunlu CSV formatındaki öğrenci verilerini sisteme aktarma ve çapraz kontrol (cross-check).

---

## 🏛️ Nesne Yönelimli Programlama (NYP) Mimarisi

- **Kapsülleme (Encapsulation):** `User.cs` içinde `PasswordHash` ve `FailedLoginAttempts` alanları `private set` yapılmış, sadece üye metotlarla kontrollü güncelleme sağlanmıştır. 5 hatalı girişte hesap 15 dakika kilitlenir.
- **Kalıtım (Inheritance):** `OgrenciDetay : User` kalıtımı ile ortak kullanıcı özellikleri temel sınıftan devralınmıştır.
- **Soyutlama (Abstraction):** `KuralKontrolcu` soyut sınıfı ve `KontrolEt()` soyut metodu tanımlanmıştır.
- **Çok Biçimlilik (Polymorphism):** `TCKimlikKontrolcu`, `TelefonKontrolcu`, `AdresKontrolcu`, `OgretimYiliKontrolcu`, `StajTarihKontrolcu`, `MuhendisBilgiKontrolcu` sınıfları soyut metodu ezerek kendi kurallarını işletir.

---

## ⚙️ Kurulum ve Çalıştırma

### Gereksinimler
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- Visual Studio 2022 veya VS Code

### Adımlar
1. Repoyu klonlayın:
   ```bash
   git clone https://github.com/ilkebingol43/proglama_lab3__Proje.git
   cd proglama_lab3__Proje
   ```
2. Bağımlılıkları yükleyin:
   ```bash
   dotnet restore
   ```
3. Projeyi çalıştırın:
   ```bash
   dotnet run
   ```
4. Tarayıcınızda açın: `https://localhost:7000` veya `http://localhost:5000`

### 🔑 Varsayılan Yönetici (Admin) Girişi
- **Kullanıcı Adı:** `admin`
- **Şifre:** `Admin123!`

---

## 👥 Geliştiriciler
- **İlke BİNGÖL**
- **Mahsun Kaçmaz**
- **OsmanCan Uslu**
