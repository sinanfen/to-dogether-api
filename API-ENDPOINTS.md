# TodoList & TodoItem API Endpoints

## Authentication
Tüm endpoint'ler için `Authorization: Bearer {token}` header'ı gereklidir.

## TodoList Endpoints

### 📋 Get My Todo Lists
```
GET /todolists
```
Kendi todo listelerimi getirir.

**Response:**
```json
[
  {
    "id": 1,
    "title": "Shopping List",
    "description": "Weekly shopping items",
    "ownerId": 2,
    "isShared": false,
    "colorCode": "#3B82F6",
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T10:00:00Z"
  }
]
```

### 👥 Get Partner's Todo Lists
```
GET /todolists/partner
```
Partnerimin todo listelerini getirir.

### ➕ Create Todo List
```
POST /todolists
```

**Body:**
```json
{
  "title": "Shopping List",
  "description": "Weekly shopping items",
  "isShared": true,
  "colorCode": "#EF4444"
}
```

### ✏️ Update Todo List
```
PUT /todolists/{id}
```

**Body:**
```json
{
  "title": "Updated Shopping List",
  "description": "Updated description",
  "isShared": false,
  "colorCode": "#10B981"
}
```

### 🗑️ Delete Todo List
```
DELETE /todolists/{id}
```

---

## TodoItem Endpoints

### 📝 Get Items in a List
```
GET /todolists/{todoListId}/items
```

**Response:**
```json
[
  {
    "id": 1,
    "title": "Buy milk",
    "description": "2% milk from store", // null olabilir
    "status": 0,
    "severity": 1,
    "order": 1,
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T10:00:00Z"
  }
]
```

### ➕ Add Item to List
```
POST /todolists/{todoListId}/items
```

**Body:**
```json
{
  "title": "Buy milk",
  "description": "2% milk from store", 
  "severity": 1
}
```

**Note:** `description` alanı opsiyoneldir (null olabilir)

### ✏️ Update Todo Item
```
PUT /todolists/{todoListId}/items/{itemId}
```

**Body:**
```json
{
  "title": "Buy organic milk",
  "description": "2% organic milk from whole foods",
  "status": 1,
  "severity": 2,
  "order": 1
}
```

**Note:** `description` alanı opsiyoneldir (null olabilir)

### 🗑️ Delete Todo Item
```
DELETE /todolists/{todoListId}/items/{itemId}
```

---

## Enums

### TodoStatus
- `0` = Pending
- `1` = Done

### TodoSeverity  
- `0` = Low
- `1` = Medium
- `2` = High

---

## Erişim Kuralları
- **isShared = false:**
  - Sadece sahibi (owner) todo list ve itemlarını düzenleyebilir, ekleyebilir, silebilir.
  - Partner sadece okuyabilir (GET ile görebilir, POST/PUT/DELETE ile değiştiremez).
- **isShared = true:**
  - Couple içindeki her iki kullanıcı da listeyi ve itemlarını tam yetkiyle yönetebilir.

## ColorCode Formatı
- Renk kodu `#RRGGBB` formatında olmalıdır (örn: `#3B82F6`, `#EF4444`, `#10B981`)
- Default değer: `#3B82F6` (mavi)
- Tüm CRUD işlemleri otomatik olarak activity log'a kaydedilir
- Base URL: `https://localhost:54696` veya `http://localhost:54697` 