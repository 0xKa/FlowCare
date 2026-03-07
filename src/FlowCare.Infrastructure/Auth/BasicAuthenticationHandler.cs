using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using FlowCare.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowCare.Infrastructure.Auth;

public class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    FlowCareDbContext db)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Basic";

    // gets called on every request by asp.net
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.NoResult();

        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var authHeader))
            return AuthenticateResult.Fail("Invalid Authorization header.");

        if (!SchemeName.Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase)) // ensure it's Basic scheme
            return AuthenticateResult.NoResult();

        if (string.IsNullOrEmpty(authHeader.Parameter))
            return AuthenticateResult.Fail("Missing credentials.");

        byte[] credentialBytes;
        try
        {
            credentialBytes = Convert.FromBase64String(authHeader.Parameter);
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Invalid Base64 in Authorization header.");
        }

        var credentials = Encoding.UTF8.GetString(credentialBytes);
        var separatorIndex = credentials.IndexOf(':');
        if (separatorIndex < 0)
            return AuthenticateResult.Fail("Invalid credential format.");

        var username = credentials[..separatorIndex];
        var password = credentials[(separatorIndex + 1)..];

        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return AuthenticateResult.Fail("Invalid username or password.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FullName", user.FullName)
        };

        if (user.BranchId is not null)
            claims.Add(new Claim("BranchId", user.BranchId));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
