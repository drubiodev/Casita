using System.Security.Claims;

namespace Casita.Api.Features.Auth;

public class HttpCurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _user;

    public HttpCurrentUser(IHttpContextAccessor accessor)
    {
        _user = accessor.HttpContext?.User
                ?? throw new InvalidOperationException("No HttpContext available.");
    }

    public bool IsAuthenticated => _user.Identity?.IsAuthenticated == true;

    public string UserId =>
        // Prefer 'sub' (mapped to NameIdentifier); fall back to Entra's 'oid' if present.
        _user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _user.FindFirstValue("oid")
        ?? throw new InvalidOperationException("Authenticated user is missing a 'sub'/'oid' claim.");

    public bool IsInRole(string role) => _user.IsInRole(role);
}
