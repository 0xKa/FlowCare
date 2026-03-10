using FlowCare.Application.DTOs;

namespace FlowCare.Application.Interfaces;

public interface IAppointmentService
{
    // Customer operations
    Task<(AppointmentResponse? Result, string? Error)> BookAsync(
        string customerId, BookAppointmentRequest request, Stream? attachment, string? attachmentFileName);
    Task<List<AppointmentResponse>> ListByCustomerAsync(string customerId);
    Task<AppointmentResponse?> GetByIdForCustomerAsync(string appointmentId, string customerId);
    Task<(bool Success, string? Error)> CancelAsync(string appointmentId, string customerId, string actorRole);
    Task<(AppointmentResponse? Result, string? Error)> RescheduleAsync(
        string appointmentId, string customerId, string newSlotId, string actorRole);

    // Staff / Manager / Admin listing
    Task<List<AppointmentResponse>> ListAsync(string role, string userId, string? branchId);
    Task<AppointmentResponse?> GetByIdAsync(string appointmentId);

    // Status update (staff+)
    Task<(bool Success, string? Error)> UpdateStatusAsync(
        string appointmentId, string newStatus, string actorId, string actorRole, string? actorBranchId);
}
