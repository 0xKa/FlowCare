# Auth and Test Requests

## Authentication

The API uses HTTP Basic Authentication.

Header format: `Authorization: Basic base64(username:password)`

Can be tested using Postman from the "Authorization" tab in the request builder by selecting "Basic Auth" and entering the username and password.

## Seeded Demo Accounts

Examples from the seed data in [example.json](../src/FlowCare.Infrastructure/Data/Seed/example.json):

- Admin
  - username: admin
  - password: Admin@123
- Branch Manager (Muscat)
  - username: mgr_muscat
  - password: Manager@123
- Branch Manager (Suhar)
  - username: mgr_suhar
  - password: Manager@123
- Staff example
  - username: staff_muscat_1
  - password: Staff@123
- Customer example
  - username: cust_ahmed
  - password: Customer@123

## Example Requests

### Build Basic Auth Header or Use Postman

PowerShell:

```powershell
[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("admin:Admin@123"))
```

### Login

```bash
curl -X POST http://localhost:5031/api/auth/login \
  -H "Authorization: Basic YWRtaW46QWRtaW5AMTIz"
```

### Register Customer (multipart with ID image)

```bash
curl -X POST http://localhost:5031/api/auth/register \
  -F "username=new_customer" \
  -F "password=Customer@123" \
  -F "fullName=New Customer" \
  -F "email=new.customer@example.com" \
  -F "phone=+96890001234" \
  -F "idImage=@/path/to/id-image.png"
```

### Book Appointment (optional attachment)

```bash
curl -X POST http://localhost:5031/api/customer/appointments \
  -H "Authorization: Basic <base64 customer creds>" \
  -F "branchId=br_muscat_001" \
  -F "serviceTypeId=svc_mus_001" \
  -F "slotId=slot_mus_001" \
  -F "attachment=@/path/to/file.pdf"
```
