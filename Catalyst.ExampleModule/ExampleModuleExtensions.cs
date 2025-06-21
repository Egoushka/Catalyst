using Catalyst.Common.Settings;
using Catalyst.Data.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Catalyst.ExampleModule;

public static class ExampleModuleExtensions
{
    public static void AddExampleModuleModule(this IServiceCollection services, DatabaseOptions databaseOptions)
    {
        services.AddDbContext<ExampleModuleDbContext>(options =>
            options.UseNpgsql(
                    databaseOptions.ConnectionStrings.DefaultConnection,
                    o => o.MigrationsHistoryTable(
                        tableName: HistoryRepository.DefaultTableName,
                        schema: "dashboard"))
                .LogTo(Console.WriteLine, LogLevel.Information)
        );

        // This ensures SaveChangesAsync operates on the correct context
        services.AddScoped<IUnitOfWork>(provider =>
            new EfUnitOfWork<ExampleModuleDbContext>(
                provider.GetRequiredService<ExampleModuleDbContext>()));

        services.AddScoped(typeof(IRepository<>), typeof(ExampleModuleRepository<>));
    }
}