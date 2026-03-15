# API Endpoints

All API endpoints are implemented as specified in the requirements. You can check Scalar API reference here: <https://flowcare-api-9wp2.onrender.com/scalar/>.

> I'm hosting the API on Render hobby plan which sleeps after 15 minutes of inactivity, so the first request after a period of inactivity may take a few seconds to wake up the instance. Subsequent requests will be fast until the next sleep cycle.

## API Capability Summary

### Public

- GET /api/branches
- GET /api/branches/{branchId}/services
- GET /api/branches/{branchId}/services/{serviceTypeId}/slots
- GET /api/branches/{branchId}/queue
- GET /api/health

### Auth

- POST /api/auth/register (multipart/form-data, includes required ID image)
- POST /api/auth/login (Basic Auth)

### Customer

- POST /api/customer/appointments (optional attachment)
- GET /api/customer/appointments
- GET /api/customer/appointments/{id}
- GET /api/customer/appointments/{id}/attachment
- PUT /api/customer/appointments/{id}/cancel
- PUT /api/customer/appointments/{id}/reschedule

### Staff, Manager, Admin

- GET /api/staff/appointments
- GET /api/staff/appointments/{id}
- PUT /api/staff/appointments/{id}/status
- GET /api/staff/appointments/{id}/attachment

### Manager, Admin

- GET /api/branches/{branchId}/slots
- POST /api/branches/{branchId}/slots
- POST /api/branches/{branchId}/slots/bulk
- PUT /api/slots/{slotId}
- DELETE /api/slots/{slotId}
- GET /api/staff
- POST /api/staff/{staffId}/services
- DELETE /api/staff/{staffId}/services/{serviceTypeId}
- GET /api/customers
- GET /api/customers/{id}

### Admin Only

- GET /api/customers/{id}/id-image
- GET /api/audit-logs/export
- POST /api/admin/cleanup
- PUT /api/settings/retention-period
- PUT /api/settings/rate-limits
- PUT /api/settings/cleanup-worker
