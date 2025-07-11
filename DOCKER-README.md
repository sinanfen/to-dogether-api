# To-Dogether API - Docker Setup

Bu dosya API projesini Docker ile çalıştırmak için gerekli talimatları içerir.

## 📋 Gereksinimler

- Docker Desktop veya Docker Engine
- Docker Compose (genellikle Docker Desktop ile birlikte gelir)

## 🚀 Hızlı Başlangıç

### 1. Projeyi Klonla
```bash
git clone [repository-url]
cd to-dogether-api
```

### 2. Docker Compose ile Çalıştır
```bash
# Servisleri arka planda başlat
docker-compose up -d

# Logları görmek için
docker-compose logs -f api
```

### 3. API'ye Erişim
- **API:** http://localhost:8080
- **Swagger:** http://localhost:8080/swagger
- **Health Check:** http://localhost:8080/health
- **PostgreSQL:** localhost:5432

## 🛠 Docker Komutları

### Servisleri Başlat
```bash
docker-compose up -d
```

### Servisleri Durdur
```bash
docker-compose down
```

### Servisleri Yeniden Başlat
```bash
docker-compose restart
```

### Logları Görüntüle
```bash
# Tüm servislerin logları
docker-compose logs -f

# Sadece API logları
docker-compose logs -f api

# Sadece PostgreSQL logları
docker-compose logs -f postgres
```

### Database'i Sıfırla
```bash
# Servisleri durdur ve volume'ları sil
docker-compose down -v

# Yeniden başlat
docker-compose up -d
```

## 🏗 Manuel Build

### Sadece Docker Image Build Et
```bash
docker build -t to-dogether-api .
```

### Manuel Çalıştır
```bash
# PostgreSQL çalıştır
docker run -d \
  --name postgres-db \
  -e POSTGRES_DB=todogether_db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres123 \
  -p 5432:5432 \
  postgres:15-alpine

# API çalıştır
docker run -d \
  --name api \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=todogether_db;Username=postgres;Password=postgres123;" \
  to-dogether-api
```

## 🔧 Konfigürasyon

### Environment Variables
Docker Compose dosyasında şu değişkenler tanımlı:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://+:8080
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=todogether_db;Username=postgres;Password=postgres123;
```

### Database Bilgileri
- **Host:** postgres (Docker network içinde)
- **Port:** 5432
- **Database:** todogether_db
- **Username:** postgres
- **Password:** postgres123

## 🏥 Health Checks

### API Health Check
```bash
curl http://localhost:8080/health
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2023-12-01T10:30:00Z",
  "environment": "Production",
  "database": "connected"
}
```

### Database Health Check
```bash
docker-compose exec postgres pg_isready -U postgres
```

## 📊 Monitoring

### Container Status
```bash
docker-compose ps
```

### Resource Usage
```bash
docker stats
```

### Container Shell Access
```bash
# API container'a gir
docker-compose exec api /bin/bash

# PostgreSQL container'a gir
docker-compose exec postgres psql -U postgres -d todogether_db
```

## 🐛 Troubleshooting

### Port Çakışması
Eğer 8080 veya 5432 portları kullanılıyorsa, docker-compose.yml dosyasında portları değiştirin:

```yaml
ports:
  - "8081:8080"  # API için
  - "5433:5432"  # PostgreSQL için
```

### Database Bağlantı Problemi
```bash
# Database loglarını kontrol et
docker-compose logs postgres

# Database'in hazır olup olmadığını kontrol et
docker-compose exec postgres pg_isready -U postgres
```

### API Başlatma Problemi
```bash
# API loglarını kontrol et
docker-compose logs api

# Container'ın çalışıp çalışmadığını kontrol et
docker-compose ps
```

### Tamamen Temizle ve Yeniden Başlat
```bash
# Tüm container, network ve volume'ları sil
docker-compose down -v --remove-orphans

# Image'ları da sil
docker rmi to-dogether-api_api

# Yeniden build et ve başlat
docker-compose up -d --build
```

## 📝 Notlar

- İlk çalıştırmada database migration'ları otomatik olarak uygulanır
- PostgreSQL verileri `postgres_data` volume'unda saklanır
- API Production modunda çalışır
- Health check'ler 30 saniyede bir çalışır
- Container'lar otomatik restart özelliği etkin

## 🔒 Güvenlik

Production ortamında şu değişiklikleri yapın:

1. **Güçlü Passwords:** PostgreSQL şifresini değiştirin
2. **Environment Variables:** Hassas bilgileri .env dosyasında saklayın
3. **Network Security:** Gereksiz portları kapatın
4. **SSL/TLS:** Reverse proxy (nginx) kullanın 