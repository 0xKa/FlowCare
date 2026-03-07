using System.Security.Claims;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Enums;

namespace FlowCare.Infrastructure.Auth;

// used to read claims we created
public class BranchAuthorizationService : IBranchAuthorizationService
{
    public Guid GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return Guid.Parse(id);
    }

    public Guid? GetBranchId(ClaimsPrincipal user)
    {
        var branchClaim = user.FindFirstValue("BranchId");
        return branchClaim is not null ? Guid.Parse(branchClaim) : null;
    }

    public string GetRole(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("Role claim not found.");
    }

    public bool CanAccessBranch(ClaimsPrincipal user, Guid branchId)
    {
        var role = GetRole(user);
        if (role == nameof(UserRole.Admin))
            return true;

        var userBranchId = GetBranchId(user);
        return userBranchId.HasValue && userBranchId.Value == branchId;
    }
}
