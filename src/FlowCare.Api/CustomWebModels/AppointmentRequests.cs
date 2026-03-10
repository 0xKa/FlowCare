namespace FlowCare.Api.CustomWebModels;

public class BookAppointmentFormRequest
{
    public required string BranchId { get; init; }
    public required string ServiceTypeId { get; init; }
    public required string SlotId { get; init; }
    public IFormFile? Attachment { get; init; }
}
