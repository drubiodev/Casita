using Casita.Infrastructure.Persistence;

namespace Casita.Api.Features.Auth;

/// <summary>
/// Bridges the API's <see cref="ICurrentUser"/> to the Infrastructure layer's
/// <see cref="ICurrentUserAccessor"/> so the connection factory can apply the
/// per-request <c>app.user_id</c> session variable consumed by RLS policies.
/// </summary>
public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? TryGetUserId()
    {
        var user = _accessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var raw = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("oid")?.Value;

        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
