# To-Dogether API

Çiftler için tasarlanmış ortak todo list yönetim sistemi API'si. JWT authentication, couple sistemi ve gerçek zamanlı todo yönetimi özellikleri ile modern bir backend çözümü.

## 🚀 Özellikler

- **JWT Authentication**: Güvenli token tabanlı kimlik doğrulama
- **Couple Sistemi**: İki kişilik çift sistemi ile invite token'ları
- **Todo List Yönetimi**: Çoklu todo list ve item desteği
- **Ortak Erişim**: Partner'lar birbirlerinin todo'larını tam yetkiyle yönetebilir
- **Profil Yönetimi**: Kullanıcı adı ve renk kodu güncelleme
- **Serilog Logging**: Detaylı loglama sistemi
- **Swagger UI**: Otomatik API dokümantasyonu

## 🛠️ Teknolojiler

- **.NET 8.0** - Minimal API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Veritabanı
- **JWT** - Authentication
- **Serilog** - Logging
- **Swagger/OpenAPI** - API Dokümantasyonu

## 📋 Gereksinimler

- .NET 8.0 SDK
- PostgreSQL
- Git

## 🔧 Kurulum

1. **Repository'yi klonlayın:**
   ```bash
   git clone https://github.com/sinanfen/to-dogether-api.git
   cd to-dogether-api
   ```

2. **Veritabanı bağlantısını yapılandırın:**
   `appsettings.json` dosyasında connection string'i güncelleyin:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=todogether;Username=your_username;Password=your_password"
     }
   }
   ```

3. **Migration'ları çalıştırın:**
   ```bash
   dotnet ef database update
   ```

4. **Uygulamayı başlatın:**
   ```bash
   dotnet run
   ```

5. **Swagger UI'a erişin:**
   ```
   https://localhost:7001/swagger
   ```

## 🔐 Güvenlik

- **SHA256 Password Hashing**: Şifreler güvenli şekilde hashlenir
- **JWT Bearer Tokens**: Güvenli authentication
- **Refresh Token Sistemi**: Otomatik token yenileme
- **CORS Yapılandırması**: Cross-origin istek desteği

## 📚 API Endpoints

### Authentication
- `POST /auth/register` - Kullanıcı kaydı
- `POST /auth/login` - Kullanıcı girişi
- `POST /auth/refresh` - Token yenileme
- `POST /auth/logout` - Çıkış yapma

### Profil Yönetimi
- `GET /profile` - Profil bilgisi getir
- `PUT /profile` - Profil güncelle

### Todo List İşlemleri
- `GET /todolists` - Kendi todo list'lerini getir
- `GET /todolists/partner` - Partner'ın todo list'lerini getir
- `POST /todolists` - Todo list oluştur
- `PUT /todolists/{id}` - Todo list güncelle
- `DELETE /todolists/{id}` - Todo list sil

### Todo Item İşlemleri
- `GET /todolists/{todoListId}/items` - Todo item'ları getir
- `POST /todolists/{todoListId}/items` - Todo item ekle
- `PUT /todolists/{todoListId}/items/{itemId}` - Todo item güncelle
- `DELETE /todolists/{todoListId}/items/{itemId}` - Todo item sil

Detaylı API dokümantasyonu için [Endpoints.md](Endpoints.md) dosyasını inceleyin.

## 🏗️ Proje Yapısı

```
to-dogether-api/
├── Data/
│   └── AppDbContext.cs          # Entity Framework context
├── DTOs/
│   ├── AuthDTOs.cs             # Authentication DTO'ları
│   └── TodoDTOs.cs             # Todo işlemleri DTO'ları
├── Middleware/
│   └── JwtMiddleware.cs        # JWT authentication middleware
├── Models/
│   ├── User.cs                 # Kullanıcı modeli
│   ├── Couple.cs               # Çift modeli
│   ├── TodoList.cs             # Todo list modeli
│   ├── TodoItem.cs             # Todo item modeli
│   └── RefreshToken.cs         # Refresh token modeli
├── Services/
│   ├── JwtService.cs           # JWT token servisi
│   └── HashService.cs          # Şifre hash servisi
├── Program.cs                  # Ana uygulama dosyası
├── appsettings.json            # Uygulama ayarları
└── README.md                   # Bu dosya
```

## 🔄 Couple Sistemi

1. **İlk Kullanıcı**: Register sırasında yeni bir couple oluşturur ve invite token alır
2. **İkinci Kullanıcı**: Register sırasında partner'ın invite token'ını kullanarak mevcut couple'a katılır
3. **Ortak Erişim**: Aynı couple içindeki kullanıcılar birbirlerinin tüm todo'larını yönetebilir

## 📝 Logging

Serilog ile aşağıdaki olaylar loglanır:
- Kullanıcı giriş/çıkış işlemleri
- Todo list/item oluşturma/güncelleme/silme
- Hata durumları
- Güvenlik olayları

Loglar hem console'a hem de PostgreSQL veritabanına yazılır.

## 🚀 Deployment

### Docker ile (Önerilen)
```bash
docker build -t to-dogether-api .
docker run -p 7001:7001 to-dogether-api
```

### Manuel Deployment
1. `dotnet publish -c Release`
2. Publish klasörünü sunucuya kopyalayın
3. `dotnet to-dogether-api.dll` ile çalıştırın

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👨‍💻 Geliştirici

**Sinan Fen** - [GitHub](https://github.com/sinanfen)

## 📞 İletişim

Proje ile ilgili sorularınız için GitHub Issues kullanabilirsiniz.

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın! 