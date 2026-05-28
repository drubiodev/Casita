namespace Casita.Infrastructure.Homes;

public interface IHomeMembershipRepository
{
    Task<bool> IsMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default);
}
