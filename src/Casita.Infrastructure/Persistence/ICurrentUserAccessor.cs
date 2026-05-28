namespace Casita.Infrastructure.Persistence;

/// <summary>
/// Resolves the current authenticated user for the database layer.
/// Default implementation returns <c>null</c> (used outside HTTP requests,
/// e.g. by the migration/seed hosted service). API hosts override this with
/// an implementation backed by <c>IHttpContextAccessor</c>.
/// </summary>
public interface ICurrentUserAccessor
{
    Guid? TryGetUserId();
}

internal sealed class NullCurrentUserAccessor : ICurrentUserAccessor
{
    public Guid? TryGetUserId() => null;
}
