# Technical Details

This file includes:

- Pagination and search implementation
- File upload and retrieval rules for customer ID images and appointment attachments
- Soft delete and cleanup logic for slots
- Audit logging structure and visibility rules
- Seeding process and idempotency

## Pagination and Search

Most list endpoints accept:

- page (default 1)
- size (default 20, max 200)
- searchTerm (optional, case-insensitive)

Response shape:

```json
{
  "data": [ ... ],
  "total": 125,
  "page": 1,
  "size": 20,
  "totalPages": 7
}
```

## File Upload and Retrieval Rules

### Customer ID Image

- Required during registration
- Allowed types: JPEG, PNG
- Max size: 5 MB
- Stored on local filesystem
- Retrieval endpoint is admin-only

### Appointment Attachment

- Optional during booking
- Allowed types: JPEG, PNG, PDF
- Max size: 5 MB
- Retrieval allowed for:
  - appointment owner (customer)
  - assigned staff
  - manager in same branch
  - admin

## Soft Delete and Cleanup

- Slots are soft-deleted by setting deleted timestamp and deactivating slot
- Soft-deleted slots are hidden from normal queries (global query filter)
- Admin can include soft-deleted slots via includeDeleted=true on branch slot listing
- Retention period is stored in DB settings (default 30 days), and can be configured by admin
- Cleanup performs hard-delete for expired soft-deleted slots
- Related appointments keep history but have SlotId set to null when slot is hard-deleted
- Cleanup is idempotent and can be run safely multiple times

## Audit Logging

Sensitive actions are logged with actor and entity context.

Audit record includes:

- action type
- actor ID and role
- entity type and entity ID
- timestamp
- metadata (JSON)

Visibility:

- Admin: all logs
- Branch Manager: only branch-related logs

Export:

- Admin can export audit logs as CSV from GET /api/audit-logs/export

## Seeding

On startup, the API:

1. applies pending migrations
1. imports seed data from configured SeedDataPath
1. skips existing records by ID (idempotent behavior)

Default local seed path in appsettings.json is:

- `../FlowCare.Infrastructure/Data/Seed/example.json`

When running with Docker or in a production environment, the seed file is copied to the container and path is adjusted to `/app/seed/example.json`.

Seed includes:

- 2 branches
- 3 service types per branch
- branch managers
- staff members
- customers
- slots
- sample appointments and audit logs
- initial system settings
