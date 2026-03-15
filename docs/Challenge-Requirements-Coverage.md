# Challenge Requirements Coverage

All mandatory and bonus requirements are implemented as specified. The following tables detail the coverage and reference key code components for each requirement.

## Mandatory Requirements Coverage

| Requirement | Status | Notes | Code Reference |
| --- | --- | --- | --- |
| PostgreSQL-backed backend API | Implemented | EF Core, Npgsql, and migrations are included. | [FlowCareDbContext.cs](src/FlowCare.Infrastructure/Data/FlowCareDbContext.cs), [DependencyInjection.cs](src/FlowCare.Infrastructure/DependencyInjection.cs) |
| Basic Authentication | Implemented | All non-public endpoints are protected by auth or role policy. | [BasicAuthenticationHandler.cs](src/FlowCare.Infrastructure/Auth/BasicAuthenticationHandler.cs), [DependencyInjection.cs](src/FlowCare.Infrastructure/DependencyInjection.cs) |
| Default Admin user | Implemented | Seed includes admin account. | [SeedDataImporter.cs](src/FlowCare.Infrastructure/Data/SeedDataImporter.cs), Seed File:  [example.json](src/FlowCare.Infrastructure/Data/Seed/example.json) |
| Roles: Admin / Branch Manager / Staff / Customer | Implemented | Enforced through authorization policies and branch scoping logic. | [UserRole.cs](src/FlowCare.Domain/Enums/UserRole.cs), [BranchAuthorizationService.cs](src/FlowCare.Infrastructure/Auth/BranchAuthorizationService.cs) |
| Public endpoints (branches/services/slots) | Implemented | Anonymous access is enabled for required listing endpoints. | [BranchesController.cs](src/FlowCare.Api/Controllers/BranchesController.cs) |
| Customer registration and login | Implemented | Registration requires ID image upload; login uses Basic Auth. | [AuthController.cs](src/FlowCare.Api/Controllers/AuthController.cs), [AuthService.cs](src/FlowCare.Infrastructure/Services/AuthService.cs) |
| Customer booking/cancel/reschedule/list/history | Implemented | Includes appointment details and attachment retrieval for owner. | [CustomerAppointmentsController.cs](src/FlowCare.Api/Controllers/CustomerAppointmentsController.cs), [AppointmentService.cs](src/FlowCare.Infrastructure/Services/AppointmentService.cs) |
| Staff/Manager/Admin appointment operations | Implemented | Listing scopes by role; staff can update status. | [StaffAppointmentsController.cs](src/FlowCare.Api/Controllers/StaffAppointmentsController.cs), [AppointmentService.cs](src/FlowCare.Infrastructure/Services/AppointmentService.cs) |
| Slot management with soft delete | Implemented | Create, update, delete (soft) with deleted timestamp. | [BranchSlotsController.cs](src/FlowCare.Api/Controllers/BranchSlotsController.cs), [SlotsController.cs](src/FlowCare.Api/Controllers/SlotsController.cs), [SlotService.cs](src/FlowCare.Infrastructure/Services/SlotService.cs) |
| Admin retention config and cleanup endpoint | Implemented | Retention stored in DB settings; cleanup hard-deletes expired soft-deleted slots. | [SettingsController.cs](src/FlowCare.Api/Controllers/SettingsController.cs), [AdminController.cs](src/FlowCare.Api/Controllers/AdminController.cs) |
| Audit logging for sensitive actions | Implemented | Appointment, slot, staff, settings, cleanup actions are logged. | [AuditLogService.cs](src/FlowCare.Infrastructure/Services/AuditLogService.cs), [AuditActionType.cs](src/FlowCare.Domain/Enums/AuditActionType.cs) |
| Audit log visibility rules | Implemented | Admin sees all; manager sees branch-scoped logs. | [AuditLogsController.cs](src/FlowCare.Api/Controllers/AuditLogsController.cs), [AuditLogQueryService.cs](src/FlowCare.Infrastructure/Services/AuditLogQueryService.cs) |
| Audit export to CSV (admin) | Implemented | Admin-only CSV endpoint is available. | [AuditLogsController.cs](src/FlowCare.Api/Controllers/AuditLogsController.cs), [AuditLogQueryService.cs](src/FlowCare.Infrastructure/Services/AuditLogQueryService.cs) |
| Seed import at startup | Implemented | Startup importer reads JSON and seeds idempotently. | [Program.cs](src/FlowCare.Api/Program.cs), [SeedDataImporter.cs](src/FlowCare.Infrastructure/Data/SeedDataImporter.cs) |
| Idempotent seeding | Implemented | Existing IDs are skipped; reruns do not duplicate rows. | [SeedDataImporter.cs](src/FlowCare.Infrastructure/Data/SeedDataImporter.cs) |
| File validation, storage, and retrieval permissions | Implemented | ID images and attachments validated by magic bytes and access checks. | [LocalFileStorageService.cs](src/FlowCare.Infrastructure/Services/LocalFileStorageService.cs), [AppointmentAttachmentService.cs](src/FlowCare.Infrastructure/Services/AppointmentAttachmentService.cs) |
| Migration scripts included | Implemented | Initial migration and snapshot are committed. | [Migrations](src/FlowCare.Infrastructure/Migrations/) |

## Challenge Bonus Requirements Coverage

| Bonus Requirement | Status | Notes | Code Reference |
| --- | --- | --- | --- |
| Pagination and search | Implemented | Implemented across listing APIs using page, size, and searchTerm. | [QueryRequests.cs](src/FlowCare.Api/CustomWebModels/QueryRequests.cs), [BranchService.cs](src/FlowCare.Infrastructure/Services/BranchService.cs), [AppointmentService.cs](src/FlowCare.Infrastructure/Services/AppointmentService.cs) |
| Live queue endpoint by branch | Implemented | Queue is based on checked-in appointments ordered by slot time. | [BranchesController.cs](src/FlowCare.Api/Controllers/BranchesController.cs), [BranchService.cs](src/FlowCare.Infrastructure/Services/BranchService.cs) |
| Customer booking and reschedule rate limits | Implemented | Limit values are configurable in settings and enforced in booking/reschedule flow. | [SettingsController.cs](src/FlowCare.Api/Controllers/SettingsController.cs), [AdminMaintenanceService.cs](src/FlowCare.Infrastructure/Services/AdminMaintenanceService.cs), [AppointmentService.cs](src/FlowCare.Infrastructure/Services/AppointmentService.cs) |
| Background cleanup worker (cron-like interval) | Implemented | Periodic background job runs cleanup and can be toggled from settings and by admin. | [SoftDeleteCleanupBackgroundService.cs](src/FlowCare.Infrastructure/BackgroundJobs/SoftDeleteCleanupBackgroundService.cs), [DependencyInjection.cs](src/FlowCare.Infrastructure/DependencyInjection.cs) |
| Dockerization and docker-compose | Implemented | Multi-stage Dockerfile and local orchestration with PostgreSQL and seed mount. | [Dockerfile](Dockerfile), [docker-compose.yml](docker-compose.yml) |
