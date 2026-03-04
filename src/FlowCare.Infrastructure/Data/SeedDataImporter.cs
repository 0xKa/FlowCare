using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FlowCare.Domain.Entities;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Infrastructure.Data;

public class SeedDataImporter(FlowCareDbContext db)
{
    // Maps seed string IDs to generated Guids for FK resolution
    private readonly Dictionary<string, Guid> _idMap = [];

    public async Task ImportAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var seedData = JsonSerializer.Deserialize<SeedData>(json)
            ?? throw new InvalidOperationException("Failed to deserialize seed data.");

        await SeedBranchesAsync(seedData.Branches);
        await SeedUsersAsync(seedData.Users);
        await SeedServiceTypesAsync(seedData.ServiceTypes);
        await SeedStaffServiceTypesAsync(seedData.StaffServiceTypes);
        // Must ignore the global query filter when checking existing slots
        await SeedSlotsAsync(seedData.Slots);
        await SeedAppointmentsAsync(seedData.Appointments);
        await SeedAuditLogsAsync(seedData.AuditLogs);
        await SeedSystemSettingsAsync();

        await db.SaveChangesAsync();
    }

    private async Task SeedBranchesAsync(List<SeedBranch> branches)
    {
        foreach (var b in branches)
        {
            var existing = await db.Branches.FirstOrDefaultAsync(x => x.SeedId == b.Id);
            if (existing != null)
            {
                _idMap[b.Id] = existing.Id;
                continue;
            }

            var entity = new Branch
            {
                Id = GenerateGuid(b.Id),
                SeedId = b.Id,
                Name = b.Name,
                City = b.City,
                Address = b.Address,
                Timezone = b.Timezone,
                IsActive = b.IsActive
            };
            _idMap[b.Id] = entity.Id;
            db.Branches.Add(entity);
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedUsersAsync(SeedUsers users)
    {
        var allUsers = users.Admin
            .Concat(users.BranchManagers)
            .Concat(users.Staff)
            .Concat(users.Customers);

        foreach (var u in allUsers)
        {
            var existing = await db.Users.FirstOrDefaultAsync(x => x.SeedId == u.Id);
            if (existing != null)
            {
                _idMap[u.Id] = existing.Id;
                continue;
            }

            var entity = new User
            {
                Id = GenerateGuid(u.Id),
                SeedId = u.Id,
                Username = u.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                Role = ParseRole(u.Role),
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                BranchId = u.BranchId != null ? ResolveId(u.BranchId) : null,
                IsActive = u.IsActive
            };
            _idMap[u.Id] = entity.Id;
            db.Users.Add(entity);
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedServiceTypesAsync(List<SeedServiceType> serviceTypes)
    {
        foreach (var st in serviceTypes)
        {
            var existing = await db.ServiceTypes.FirstOrDefaultAsync(x => x.SeedId == st.Id);
            if (existing != null)
            {
                _idMap[st.Id] = existing.Id;
                continue;
            }

            var entity = new ServiceType
            {
                Id = GenerateGuid(st.Id),
                SeedId = st.Id,
                BranchId = ResolveId(st.BranchId),
                Name = st.Name,
                Description = st.Description,
                DurationMinutes = st.DurationMinutes,
                IsActive = st.IsActive
            };
            _idMap[st.Id] = entity.Id;
            db.ServiceTypes.Add(entity);
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedStaffServiceTypesAsync(List<SeedStaffServiceType> staffServiceTypes)
    {
        foreach (var sst in staffServiceTypes)
        {
            var staffId = ResolveId(sst.StaffId);
            var serviceTypeId = ResolveId(sst.ServiceTypeId);
            var existing = await db.StaffServiceTypes
                .AnyAsync(x => x.StaffId == staffId && x.ServiceTypeId == serviceTypeId);
            if (existing) continue;

            db.StaffServiceTypes.Add(new StaffServiceType
            {
                StaffId = staffId,
                ServiceTypeId = serviceTypeId
            });
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedSlotsAsync(List<SeedSlot> slots)
    {
        foreach (var s in slots)
        {
            // IgnoreQueryFilters to find even soft-deleted slots
            var existing = await db.Slots.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SeedId == s.Id);
            if (existing != null)
            {
                _idMap[s.Id] = existing.Id;
                continue;
            }

            var entity = new Slot
            {
                Id = GenerateGuid(s.Id),
                SeedId = s.Id,
                BranchId = ResolveId(s.BranchId),
                ServiceTypeId = ResolveId(s.ServiceTypeId),
                StaffId = s.StaffId != null ? ResolveId(s.StaffId) : null,
                StartAt = s.StartAt.ToUniversalTime(),
                EndAt = s.EndAt.ToUniversalTime(),
                Capacity = s.Capacity,
                IsActive = s.IsActive
            };
            _idMap[s.Id] = entity.Id;
            db.Slots.Add(entity);
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedAppointmentsAsync(List<SeedAppointment> appointments)
    {
        foreach (var a in appointments)
        {
            var existing = await db.Appointments.FirstOrDefaultAsync(x => x.SeedId == a.Id);
            if (existing != null)
            {
                _idMap[a.Id] = existing.Id;
                continue;
            }

            var entity = new Appointment
            {
                Id = GenerateGuid(a.Id),
                SeedId = a.Id,
                CustomerId = ResolveId(a.CustomerId),
                BranchId = ResolveId(a.BranchId),
                ServiceTypeId = ResolveId(a.ServiceTypeId),
                SlotId = ResolveId(a.SlotId),
                StaffId = a.StaffId != null ? ResolveId(a.StaffId) : null,
                Status = ParseStatus(a.Status),
                CreatedAt = a.CreatedAt.ToUniversalTime()
            };
            _idMap[a.Id] = entity.Id;
            db.Appointments.Add(entity);
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedAuditLogsAsync(List<SeedAuditLog> auditLogs)
    {
        foreach (var al in auditLogs)
        {
            var existing = await db.AuditLogs.FirstOrDefaultAsync(x => x.SeedId == al.Id);
            if (existing != null) continue;

            var entity = new AuditLog
            {
                Id = GenerateGuid(al.Id),
                SeedId = al.Id,
                ActorId = ResolveId(al.ActorId),
                ActorRole = al.ActorRole,
                ActionType = al.ActionType,
                EntityType = al.EntityType,
                EntityId = al.EntityId,
                Timestamp = al.Timestamp.ToUniversalTime(),
                Metadata = al.Metadata != null ? JsonSerializer.Serialize(al.Metadata) : null
            };
            db.AuditLogs.Add(entity);
        }
        await db.SaveChangesAsync();
    }

    private async Task SeedSystemSettingsAsync()
    {
        var key = "SoftDeleteRetentionDays";
        var existing = await db.SystemSettings.FindAsync(key);
        if (existing == null)
        {
            db.SystemSettings.Add(new SystemSetting { Key = key, Value = "30" });
        }
    }

    private Guid ResolveId(string seedId)
    {
        if (_idMap.TryGetValue(seedId, out var guid))
            return guid;

        // Generate deterministically if not yet in map (may be forward reference)
        guid = GenerateGuid(seedId);
        _idMap[seedId] = guid;
        return guid;
    }

    /// <summary>
    /// Generates a deterministic GUID from a seed string using SHA256.
    /// This ensures the same seed ID always maps to the same GUID across runs.
    /// </summary>
    private static Guid GenerateGuid(string seedId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seedId));
        // Take first 16 bytes of hash for Guid
        return new Guid(hash.AsSpan(0, 16));
    }

    private static UserRole ParseRole(string role) => role switch
    {
        "ADMIN" => UserRole.Admin,
        "BRANCH_MANAGER" => UserRole.BranchManager,
        "STAFF" => UserRole.Staff,
        "CUSTOMER" => UserRole.Customer,
        _ => throw new ArgumentException($"Unknown role: {role}")
    };

    private static AppointmentStatus ParseStatus(string status) => status switch
    {
        "BOOKED" => AppointmentStatus.Booked,
        "CHECKED_IN" => AppointmentStatus.CheckedIn,
        "NO_SHOW" => AppointmentStatus.NoShow,
        "COMPLETED" => AppointmentStatus.Completed,
        "CANCELLED" => AppointmentStatus.Cancelled,
        "RESCHEDULED" => AppointmentStatus.Rescheduled,
        _ => throw new ArgumentException($"Unknown appointment status: {status}")
    };
}
