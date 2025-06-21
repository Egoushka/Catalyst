using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalyst.ExampleModule;

// TODO: Replace with configuration for each module
public class ExampleModuleContextFactory : IDesignTimeDbContextFactory<ExampleModuleDbContext>
{
    public ExampleModuleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ExampleModuleDbContext>();
        optionsBuilder.UseNpgsql("");

        return new ExampleModuleDbContext(optionsBuilder.Options);
    }
}