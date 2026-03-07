using System.Security.Claims;

namespace FlowCare.Application.Interfaces;

public interface IBranchAuthorizationService
{
    /// <summary>
    /// Returns the user ID from the claims principal.
    /// </summary>
    string GetUserId(ClaimsPrincipal user);

    /// <summary>
    /// Returns the branch ID the user is assigned to, or null for admin/customer.
    /// </summary>
    string? GetBranchId(ClaimsPrincipal user);

    /// <summary>
    /// Returns the user's role as a string.
    /// </summary>
    string GetRole(ClaimsPrincipal user);

    /// <summary>
    /// Checks whether the user is authorized to access data for the specified branch.
    /// Admins can access all branches. Managers and staff can only access their own.
    /// </summary>
    bool CanAccessBranch(ClaimsPrincipal user, string branchId);
}
