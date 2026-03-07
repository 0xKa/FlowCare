using System.Security.Claims;
using FlowCare.Application.Interfaces;
using FlowCare.Domain.Enums;

namespace FlowCare.Infrastructure.Auth;

// used to read claims we created
public class BranchAuthorizationService : IBranchAuthorizationService
{
    public string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
    }

    public string? GetBranchId(ClaimsPrincipal user)
    {
        return user.FindFirstValue("BranchId");
    }

    public string GetRole(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("Role claim not found.");
    }

    public bool CanAccessBranch(ClaimsPrincipal user, string branchId)
    {
        var role = GetRole(user);
        if (role == nameof(UserRole.Admin))
            return true;

        var userBranchId = GetBranchId(user);
        return userBranchId is not null && userBranchId == branchId;
    }
}
