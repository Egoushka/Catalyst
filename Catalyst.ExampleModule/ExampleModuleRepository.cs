using Catalyst.Data;
using Catalyst.Data.Abstraction;

namespace Catalyst.ExampleModule;

public class ExampleModuleRepository<T> : EfRepository<T>
    where T : BaseEntity
{
    public ExampleModuleRepository(ExampleModuleDbContext context) : base(context)
    {
    }
}