using System.ComponentModel.DataAnnotations;
using Catalyst.Data.Abstraction;

namespace Catalyst.ExampleModule.Example.Domain.Models;

public class ServiceOrganization : BaseEntity
{
    [StringLength(50)]
    public string Name { get; init; } = null!;
}