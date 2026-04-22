# Blog Clean Architecture Backend (.NET 8 + PostgreSQL)

Clean Architecture: Domain / Application / Infrastructure / WebApi  
Features: EF Core, Identity + JWT, Full-Text Search (tsvector + GIN), Autocomplete (pg_trgm), Comments, Tags, Related

## 1) เตรียมฐานข้อมูล (ไม่ใช้ Docker)
- ติดตั้ง PostgreSQL (port 5432)
- สร้าง DB และเปิด extension
```sql
-- psql -h localhost -U postgres
CREATE DATABASE blogdb;
\c blogdb
CREATE EXTENSION IF NOT EXISTS pg_trgm;
```

## 2) ตั้งค่าแอป
แก้ `src/Blog.WebApi/appsettings.json`:
```json
{
  "ConnectionStrings": { "Default": "Host=localhost;Port=5432;Database=blogdb;Username=postgres;Password=postgres" },
  "Jwt": { "Issuer": "BlogApi", "Audience": "BlogApiAudience", "Key": "REPLACE_THIS_WITH_A_LONG_RANDOM_SECRET_64+", "ExpirationMinutes": 120 }
}
```

## 3) สร้าง/Apply Migration
```bash
# ที่โฟลเดอร์ root (มี src/)
dotnet restore

# สร้าง migrations (ครั้งแรก ไม่ต้องแก้ไฟล์เพิ่มเติม)
dotnet ef migrations add InitialCreate -p src/Blog.Infrastructure -s src/Blog.WebApi

dotnet ef migrations add AddFullTextSearch -p src/Blog.Infrastructure -s src/Blog.WebApi

# Apply migrations ลงฐานข้อมูล
dotnet ef database update -p src/Blog.Infrastructure -s src/Blog.WebApi
```
> ถ้ามี error `CREATE EXTENSION` → เปิดใน psql ด้วย superuser: `CREATE EXTENSION IF NOT EXISTS pg_trgm;`

## 4) รัน API
```bash
cd src/Blog.WebApi
dotnet run
# Swagger: http://localhost:5000/swagger
```

## 5) Endpoints หลัก
- `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`
- `GET /api/articles`, `GET /api/articles/slug/{slug}`, `GET /api/articles/{id}/related`
- `POST /api/articles`, `PUT /api/articles/{id}`, `POST /api/articles/{id}/publish` (ต้อง role `author`/`admin`)
- `GET /api/articles/{articleId}/comments`, `POST /api/articles/{articleId}/comments`
- `GET /api/search?q=`, `GET /api/search/suggest?prefix=`

## 6) หมายเหตุ
- ใช้ Identity (AppUser/AppRole) ชนิดคีย์ long
- Search ใช้ `tsvector + GIN` และ `pg_trgm` สำหรับ autocomplete
- Seed roles/users ทำใน `Program.cs` (admin/author) และ seed บทความใน `DbInitializer.SeedAsync`
