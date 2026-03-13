using System.ComponentModel.DataAnnotations;

namespace FlowCare.Api.CustomWebModels;

public class BookAppointmentFormRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string BranchId { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string ServiceTypeId { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string SlotId { get; init; }

    public IFormFile? Attachment { get; init; }
}
