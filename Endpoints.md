# To-Dogether API Endpoints

## Kimlik Doğrulama (Authentication)

### Kayıt Ol (Register)
**POST** `/auth/register`

Kullanıcı kaydı yapar. İsteğe bağlı olarak partner'ın invite token'ı ile mevcut bir couple'a katılabilir.

**Request Body:**
```json
{
  "username": "kullaniciadi",
  "password": "sifre",
  "colorCode": "#FF5733",
  "inviteToken": "abc123def456" // İsteğe bağlı
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123def456ghi789",
  "username": "kullaniciadi",
  "userId": 1
}
```

**Hata Yanıtları:**
- `400` - Kullanıcı adı zaten kullanılıyor
- `400` - Geçersiz invite token

---

### Giriş Yap (Login)
**POST** `/auth/login`

Kullanıcı girişi yapar ve JWT token'ları döner.

**Request Body:**
```json
{
  "username": "kullaniciadi",
  "password": "sifre"
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123def456ghi789",
  "username": "kullaniciadi",
  "userId": 1
}
```

**Hata Yanıtları:**
- `400` - Kullanıcı bulunamadı
- `400` - Hatalı şifre

---

### Token Yenile (Refresh)
**POST** `/auth/refresh`

Access token'ı yeniler.

**Request Body:**
```json
{
  "refreshToken": "abc123def456ghi789"
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "xyz789abc123def456"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz refresh token
- `400` - Refresh token süresi dolmuş

---

## Kullanıcı Profil (User Profile)

### Profil Bilgisi Getir
**GET** `/profile`

Kullanıcının profil bilgilerini getirir.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response (200):**
```json
{
  "id": 1,
  "username": "kullaniciadi",
  "colorCode": "#FF5733",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token

---

### Profil Güncelle
**PUT** `/profile`

Kullanıcının profil bilgilerini günceller.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Request Body:**
```json
{
  "username": "yeni_kullaniciadi",
  "colorCode": "#33FF57"
}
```

**Response (200):**
```json
{
  "id": 1,
  "username": "yeni_kullaniciadi",
  "colorCode": "#33FF57",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `400` - Kullanıcı adı zaten kullanılıyor

---

## Todo List İşlemleri

### Kendi Todo List'lerini Getir
**GET** `/todolists`

Kullanıcının kendi oluşturduğu todo list'lerini getirir.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response (200):**
```json
[
  {
    "id": 1,
    "title": "Ev alışverişi",
    "description": "Market alışverişi listesi",
    "ownerId": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": 2,
    "title": "İndirilecek oyunlar",
    "description": "Steam'den indirilecek oyunlar",
    "ownerId": 1,
    "createdAt": "2024-01-15T11:00:00Z",
    "updatedAt": "2024-01-15T11:00:00Z"
  }
]
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token

---

### Partner'ın Todo List'lerini Getir
**GET** `/todolists/partner`

Partner'ın oluşturduğu todo list'lerini getirir.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response (200):**
```json
[
  {
    "id": 3,
    "title": "Temizlik yapılacaklar",
    "description": "Ev temizliği listesi",
    "ownerId": 2,
    "createdAt": "2024-01-15T12:00:00Z",
    "updatedAt": "2024-01-15T12:00:00Z"
  }
]
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `400` - Henüz bir couple'a ait değilsiniz

---

### Todo List Oluştur
**POST** `/todolists`

Yeni bir todo list oluşturur.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Request Body:**
```json
{
  "title": "Ev alışverişi",
  "description": "Market alışverişi listesi"
}
```

**Response (201):**
```json
{
  "id": 1,
  "title": "Ev alışverişi",
  "description": "Market alışverişi listesi",
  "ownerId": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token

---

### Todo List Güncelle
**PUT** `/todolists/{id}`

Mevcut bir todo list'i günceller.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Request Body:**
```json
{
  "title": "Güncellenmiş başlık",
  "description": "Güncellenmiş açıklama"
}
```

**Response (200):**
```json
{
  "id": 1,
  "title": "Güncellenmiş başlık",
  "description": "Güncellenmiş açıklama",
  "ownerId": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T11:00:00Z"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `404` - Todo list bulunamadı

---

### Todo List Sil
**DELETE** `/todolists/{id}`

Bir todo list'i siler.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response (204):**
```
No Content
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `404` - Todo list bulunamadı

---

## Todo Item İşlemleri

### Bir Todo List'in Item'larını Getir
**GET** `/todolists/{todoListId}/items`

Belirli bir todo list'in içindeki tüm item'ları getirir.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response (200):**
```json
[
  {
    "id": 1,
    "title": "Süt al",
    "description": "Yarım yağlı süt",
    "status": "Pending",
    "severity": "Medium",
    "order": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": 2,
    "title": "Ekmek al",
    "description": "Tam buğday ekmeği",
    "status": "Done",
    "severity": "Low",
    "order": 2,
    "createdAt": "2024-01-15T10:31:00Z",
    "updatedAt": "2024-01-15T11:00:00Z"
  }
]
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `404` - Todo list bulunamadı
- `400` - Bu todo list'e erişim yetkiniz yok

---

### Todo Item Ekle
**POST** `/todolists/{todoListId}/items`

Belirli bir todo list'e yeni item ekler.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Request Body:**
```json
{
  "title": "Yumurta al",
  "description": "15'li yumurta paketi",
  "severity": "Medium"
}
```

**Response (201):**
```json
{
  "id": 3,
  "title": "Yumurta al",
  "description": "15'li yumurta paketi",
  "status": "Pending",
  "severity": "Medium",
  "order": 3,
  "createdAt": "2024-01-15T12:00:00Z",
  "updatedAt": "2024-01-15T12:00:00Z"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `404` - Todo list bulunamadı
- `400` - Bu todo list'e erişim yetkiniz yok

---

### Todo Item Güncelle
**PUT** `/todolists/{todoListId}/items/{itemId}`

Belirli bir todo item'ı günceller.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Request Body:**
```json
{
  "title": "Güncellenmiş başlık",
  "description": "Güncellenmiş açıklama",
  "status": "Done",
  "severity": "High",
  "order": 1
}
```

**Response (200):**
```json
{
  "id": 1,
  "title": "Güncellenmiş başlık",
  "description": "Güncellenmiş açıklama",
  "status": "Done",
  "severity": "High",
  "order": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T12:00:00Z"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `404` - Todo list bulunamadı
- `404` - Todo item bulunamadı
- `400` - Bu todo list'e erişim yetkiniz yok

---

### Todo Item Sil
**DELETE** `/todolists/{todoListId}/items/{itemId}`

Belirli bir todo item'ı siler.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Response (204):**
```
No Content
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `404` - Todo list bulunamadı
- `404` - Todo item bulunamadı
- `400` - Bu todo list'e erişim yetkiniz yok

---

## Partner İşlemleri

### Partner Genel Bakış
**GET** `/partner/overview`

Partner'ın tüm bilgilerini, todo listelerini ve itemlerini tek seferde getirir.

**Headers:**
```
Authorization: Bearer <jwt_token>
```

**Durum 1: Partner Varsa (200 OK)**
```json
{
  "id": 2,
  "username": "partner_username",
  "colorCode": "#EF4444",
  "createdAt": "2024-01-15T10:30:00Z",
  "todoLists": [
    {
      "id": 3,
      "title": "Ev temizliği",
      "description": "Haftalık temizlik listesi",
      "ownerId": 2,
      "createdAt": "2024-01-15T12:00:00Z",
      "updatedAt": "2024-01-15T12:00:00Z",
      "items": [
        {
          "id": 5,
          "title": "Banyo temizliği",
          "description": "Duşakabin ve lavabo temizliği",
          "status": "Pending",
          "severity": "Medium",
          "order": 1,
          "createdAt": "2024-01-15T12:00:00Z",
          "updatedAt": "2024-01-15T12:00:00Z"
        }
      ]
    }
  ]
}
```

**Durum 2: Partner Yoksa (200 OK)**
```json
{
  "message": "Henüz partner'ınız yok. Invite token'ınızı paylaşarak partner'ınızı davet edebilirsiniz.",
  "inviteToken": "abc123def456"
}
```

**Hata Yanıtları:**
- `400` - Geçersiz veya eksik JWT token
- `400` - Henüz bir couple'a ait değilsiniz

**Notlar:**
- Partner yoksa invite token döner, böylece kullanıcı partner'ını davet edebilir
- Partner varsa tüm todo listeleri ve itemleri ile birlikte döner
- Response tipi partner'ın varlığına göre değişir

---

## Veri Tipleri

### TodoStatus Enum
```json
"Pending" | "Done"
```

### TodoSeverity Enum
```json
"Low" | "Medium" | "High"
```

---

## Genel Notlar

### Yetkilendirme
- Tüm endpoint'ler JWT Bearer token ile korunur
- Token'lar `Authorization: Bearer <token>` header'ında gönderilmelidir

### Couple Sistemi
- Kullanıcılar register sırasında invite token ile mevcut bir couple'a katılabilir
- Aynı couple içindeki kullanıcılar birbirlerinin tüm todo list ve item'larını yönetebilir
- Couple olmayan kullanıcılar sadece kendi todo'larını yönetebilir

### Hata Mesajları
- Tüm hata mesajları Türkçe olarak döner
- HTTP status kodları standart REST kurallarına uyar

### Tarih Formatı
- Tüm tarih alanları ISO 8601 formatında (UTC) döner
- Örnek: `"2024-01-15T10:30:00Z"` 