using Casita.Infrastructure.Homes;
using Casita.Infrastructure.Persistence;
using Casita.Infrastructure.Tickets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Casita.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>()
            .Bind(configuration.GetSection(PostgresOptions.SectionName))
            .ValidateOnStart();

        // Default no-op user accessor; API host overrides with HTTP-aware impl.
        services.TryAddSingleton<ICurrentUserAccessor, NullCurrentUserAccessor>();

        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IHomeMembershipRepository, HomeMembershipRepository>();

        services.AddHostedService<DatabaseInitializer>();

        return services;
    }
}
