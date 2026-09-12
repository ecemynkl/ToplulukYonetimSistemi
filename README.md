 Topluluk Yönetim Sistemi

Üniversite topluluklarının dijital ortamda yönetilmesini sağlayan **ASP.NET Core MVC** tabanlı bir web uygulaması.

Bu proje, Bursa Uludağ Üniversitesi Orhangazi Yeniköy Asil Çelik Meslek Yüksekokulu için geliştirilmiş bir **Topluluk Yönetim Sistemi** örneğidir. Sistem; toplulukların, üyelerin, etkinliklerin ve duyuruların tek bir panel üzerinden yönetilmesini amaçlamaktadır.




 Admin Paneli

* Topluluk ekleme, düzenleme ve silme
* Üye yönetimi
* Etkinlik oluşturma ve düzenleme
* Duyuru paylaşma
* Dashboard üzerinde istatistik kartları
* Yönetim panelinden tüm içerikleri kontrol edebilme

 Öğrenci Paneli

* Toplulukları görüntüleme
* Etkinlikleri inceleme
* Duyuruları görüntüleme
* Topluluk detay sayfalarını görüntüleme
* Kampüs duyurularını takip edebilme

---

 Kullanılan Teknolojiler

* ASP.NET Core MVC
* C#
* Entity Framework Core
* SQL Server
* Bootstrap 5
* HTML5
* CSS3
* JavaScript
* Font Awesome

---

 Veritabanı Yapısı

Projede aşağıdaki temel tablolar kullanılmaktadır:

| Tablo             | Açıklama                               |
| ----------------- | -------------------------------------- |
| Communities       | Topluluk bilgileri                     |
| Members           | Üye bilgileri                          |
| MemberCommunities | Üye – Topluluk ilişkisi (Many-to-Many) |
| Events            | Etkinlik bilgileri                     |
| Announcements     | Duyurular                              |

> **MemberCommunities** tablosu sayesinde bir öğrenci birden fazla topluluğa katılabilir ve bir toplulukta birden fazla öğrenci bulunabilir.

---

 Sistem Modülleri

* Dashboard
* Topluluk Yönetimi
* Üye Yönetimi
* Etkinlik Yönetimi
* Duyuru Yönetimi
* Hakkımızda Sayfası
* İletişim Sayfası


 Kurulum

1. Projeyi bilgisayarınıza indirin.
2. ZIP dosyasını ayıklayın.
3. `ToplulukYonetimSistemi.sln` dosyasını Visual Studio ile açın.
4. SQL Server bağlantısını `appsettings.json` dosyasında yapılandırın.
5. Gerekirse Migration'ları çalıştırın.

```powershell
Update-Database
```

---

 Projenin Amacı

Bu sistem, üniversite topluluklarının yönetim süreçlerini dijitalleştirerek topluluk, üye, etkinlik ve duyuru işlemlerini tek bir platform üzerinden yönetilebilir hale getirmeyi amaçlamaktadır.

---

 Geliştirici

Ecem Yünkül

Bilgisayar Programcılığı
Bursa Uludağ Üniversitesi 
