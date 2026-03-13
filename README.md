# FlowCare

Rihal Codestacker 2026 (Backend): Queue & Appointment Booking System

## Overview

FlowCare is a backend API for branch-based appointment booking and queue operations with role-based access control.

Roles:

- Admin
- BranchManager
- Staff
- Customer

Tech stack:

- .NET 10 Web API
- PostgreSQL
- EF Core + Npgsql
- Basic Authentication

## Prerequisites

- .NET SDK 10
- PostgreSQL running locally or remotely

## Configuration

Set values in [src/FlowCare.Api/appsettings.json](src/FlowCare.Api/appsettings.json) or environment variables.

Required config keys:

- `ConnectionStrings__DefaultConnection`
- `SeedDataPath` (example: `src/FlowCare.Infrastructure/Data/Seed/example.json`)
- `FileStorage__BasePath` (example: `uploads`)

Example PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Database=FlowCareDb;Username=postgres;Password=postgres"
$env:SeedDataPath = "src/FlowCare.Infrastructure/Data/Seed/example.json"
$env:FileStorage__BasePath = "uploads"
```

## Setup

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project src/FlowCare.Api --launch-profile https
```

The app applies migrations and imports seed data on startup.

## Database Migrations

Create migration:

```powershell
dotnet ef migrations add <Name> -p src/FlowCare.Infrastructure -s src/FlowCare.Api
```

Apply migration:

```powershell
dotnet ef database update -p src/FlowCare.Infrastructure -s src/FlowCare.Api
```

## Seeding

- Seed import is idempotent.
- The startup importer checks existing IDs before insert.
- System setting `SoftDeleteRetentionDays` is seeded if missing.

## Example API Usage

Base URL (https profile): `https://localhost:7154`

Public:

```bash
curl -k https://localhost:7154/api/branches
```

Customer register:

```bash
curl -k -X POST https://localhost:7154/api/auth/register \
 -F "username=cust1" \
 -F "password=Pass@123" \
 -F "fullName=Customer One" \
 -F "email=cust1@example.com" \
 -F "idImage=@./id.png"
```

Customer list own appointments:

```bash
curl -k -u cust1:Pass@123 https://localhost:7154/api/customer/appointments
```

Staff list appointments:

```bash
curl -k -u staff1:Staff@123 https://localhost:7154/api/staff/appointments
```

Manager create slot:

```bash
curl -k -u manager1:Manager@123 -X POST https://localhost:7154/api/branches/br_muscat_001/slots \
 -H "Content-Type: application/json" \
 -d '{"serviceTypeId":"svc_muscat_001","staffId":"usr_staff_001","startAt":"2026-03-20T08:00:00Z","endAt":"2026-03-20T08:30:00Z","capacity":1}'
```

Admin cleanup soft-deleted slots:

```bash
curl -k -u admin:Admin@123 -X POST https://localhost:7154/api/admin/cleanup
```

Audit log CSV export (admin):

```bash
curl -k -u admin:Admin@123 https://localhost:7154/api/audit-logs/export -o audit-logs.csv
```

## API Docs

In development, Scalar docs are available at:

- `/scalar/v1`
