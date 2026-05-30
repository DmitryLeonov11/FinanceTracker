using System.Data.Common;
using FinanceTracker.Infrastructure.Persistence;
using FinanceTracker.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinanceTracker.Api.FunctionalTests.Infrastructure;

/// <summary>
/// Hosts the API in-process backed by an in-memory SQLite database.
/// SQLite uses real SQL semantics (real transactions, constraint checks) — closer to Postgres
/// than EF Core InMemory, while avoiding any external dependency.
/// </summary>
public sealed class FinanceTrackerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DbConnection _connection;
    private bool _schemaCreated;
    private readonly object _schemaLock = new();

    public FinanceTrackerWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open(); // must stay open for the lifetime of the in-memory DB

        // Program.cs validates Jwt:SigningKey and Cors:Origins at builder-time, before
        // WebApplicationFactory's ConfigureAppConfiguration kicks in. Process env vars are
        // picked up by WebApplication.CreateBuilder() via the default env-var config source.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", "Data Source=:memory:");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "TEST_KEY_AT_LEAST_32_CHARACTERS_LONG_FOR_HS256");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "FinanceTracker.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "FinanceTracker.Tests.Clients");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", "5");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", "1");
        Environment.SetEnvironmentVariable("Cors__Origins__0", "http://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Wipe all EF-related registrations from the production stack (Npgsql provider services
            // are scattered across DbContextOptions, ApplicationDbContext, and EF internal services).
            var efDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(ApplicationDbContext) ||
                    (d.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ?? false))
                .ToList();
            foreach (var d in efDescriptors) services.Remove(d);

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlite(_connection);
                options.AddInterceptors(
                    sp.GetRequiredService<AuditableEntityInterceptor>(),
                    sp.GetRequiredService<DispatchDomainEventsInterceptor>());
            });

            services.AddScoped<FinanceTracker.Application.Common.Interfaces.IApplicationDbContext>(
                sp => sp.GetRequiredService<ApplicationDbContext>());
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        EnsureSchema();
        base.ConfigureClient(client);
    }

    private void EnsureSchema()
    {
        if (_schemaCreated) return;
        lock (_schemaLock)
        {
            if (_schemaCreated) return;
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            _schemaCreated = true;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }
        base.Dispose(disposing);
    }
}
