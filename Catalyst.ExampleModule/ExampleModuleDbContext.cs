using Catalyst.ExampleModule.Example.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Type = Catalyst.ExampleModule.Example.Domain.Models.Type;

namespace Catalyst.ExampleModule;

public sealed class ExampleModuleDbContext : DbContext
{
    public ExampleModuleDbContext(DbContextOptions<ExampleModuleDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExampleEntity> ExampleEntities { get; set; } = null!;
    public DbSet<Type> Types { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dashboard");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExampleModuleDbContext).Assembly);
    }
}