namespace Casita.Api.Features.Auth;

public interface ICurrentUser
{
    string UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
