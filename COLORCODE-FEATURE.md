# TodoList ColorCode Özelliği

## Genel Bakış
TodoList'lere renk kodu özelliği eklendi. Her todo list'in kendine özel bir rengi olabilir ve bu renk frontend'de görsel olarak kullanılabilir.

## Teknik Detaylar

### Model Değişiklikleri
```csharp
// Models/TodoList.cs
public class TodoList
{
    // ... diğer alanlar ...
    public string ColorCode { get; set; } = "#3B82F6"; // Default mavi
    // ... diğer alanlar ...
}
```

### DTO Değişiklikleri
```csharp
// DTOs/TodoDTOs.cs
public record CreateTodoListRequest(
    string Title, 
    string Description, 
    bool IsShared = false, 
    string ColorCode = "#3B82F6"
);

public record UpdateTodoListRequest(
    string Title, 
    string Description, 
    bool IsShared, 
    string ColorCode
);

public record TodoListResponse(
    int Id, 
    string Title, 
    string Description, 
    int OwnerId, 
    bool IsShared, 
    string ColorCode, 
    DateTime CreatedAt, 
    DateTime UpdatedAt
);
```

## API Endpoint'leri

### POST /todolists - Yeni Liste Oluşturma
```json
{
  "title": "Shopping List",
  "description": "Weekly shopping items",
  "isShared": true,
  "colorCode": "#EF4444"
}
```

**Response:**
```json
{
  "id": 1,
  "title": "Shopping List",
  "description": "Weekly shopping items",
  "ownerId": 2,
  "isShared": true,
  "colorCode": "#EF4444",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z"
}
```

### PUT /todolists/{id} - Liste Güncelleme
```json
{
  "title": "Updated Shopping List",
  "description": "Updated description",
  "isShared": false,
  "colorCode": "#10B981"
}
```

### GET /todolists - Kendi Listelerim
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

## Renk Kodu Formatı

### Geçerli Format
- **Hex formatı**: `#RRGGBB`
- **Örnekler**: 
  - `#3B82F6` (mavi - default)
  - `#EF4444` (kırmızı)
  - `#10B981` (yeşil)
  - `#F59E0B` (sarı)
  - `#8B5CF6` (mor)

### Default Değer
- Yeni oluşturulan listeler için default renk: `#3B82F6` (mavi)
- Eğer `colorCode` belirtilmezse bu renk kullanılır

## Veritabanı Değişiklikleri

### Migration
```sql
-- AddColorCodeToTodoList migration
ALTER TABLE "TodoLists" ADD COLUMN "ColorCode" character varying(7) NOT NULL DEFAULT '#3B82F6';
```

### Kolon Özellikleri
- **Tip**: `varchar(7)` (6 karakter hex + 1 karakter #)
- **Default**: `#3B82F6`
- **Nullable**: false

## Frontend Kullanım Önerileri

### CSS Kullanımı
```css
.todo-list {
    border-left: 4px solid var(--list-color);
    background-color: var(--list-color-light);
}

.todo-list-header {
    color: var(--list-color);
}
```

### JavaScript Kullanımı
```javascript
// Renk kodunu CSS variable olarak ayarlama
function setListColor(colorCode) {
    document.documentElement.style.setProperty('--list-color', colorCode);
    
    // Açık ton oluşturma (opacity ile)
    const lightColor = colorCode + '20'; // %20 opacity
    document.documentElement.style.setProperty('--list-color-light', lightColor);
}
```

## Örnek Kullanım Senaryoları

### 1. Kategori Bazlı Renklendirme
```json
{
  "title": "Work Tasks",
  "colorCode": "#EF4444"  // Kırmızı - acil işler
}

{
  "title": "Personal Goals", 
  "colorCode": "#10B981"  // Yeşil - kişisel hedefler
}

{
  "title": "Shopping List",
  "colorCode": "#F59E0B"  // Sarı - alışveriş
}
```

### 2. Öncelik Bazlı Renklendirme
```json
{
  "title": "High Priority",
  "colorCode": "#EF4444"  // Kırmızı - yüksek öncelik
}

{
  "title": "Medium Priority",
  "colorCode": "#F59E0B"  // Sarı - orta öncelik
}

{
  "title": "Low Priority",
  "colorCode": "#10B981"  // Yeşil - düşük öncelik
}
```

## Notlar

- ✅ Renk kodu her zaman `#` ile başlamalı
- ✅ 6 karakterlik hex kodu (0-9, A-F)
- ✅ Büyük/küçük harf duyarlı değil
- ✅ Default değer: `#3B82F6` (mavi)
- ✅ Tüm TodoList CRUD işlemlerinde desteklenir
- ✅ Activity log'da renk değişiklikleri kaydedilir 