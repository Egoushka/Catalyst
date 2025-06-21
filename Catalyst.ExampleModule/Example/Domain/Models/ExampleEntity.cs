using System.ComponentModel.DataAnnotations;
using Catalyst.Data.Abstraction;
using Catalyst.ExampleModule.Example.Domain.Enums;

namespace Catalyst.ExampleModule.Example.Domain.Models;

public class ExampleEntity : BaseEntity
{
    public int TypeId { get; set; }

    [StringLength(50)]
    public string SerialNumber { get; set; } = default!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }

    public ExampleEntityStatus GeneralStatus { get; set; }
    public Type Type { get; init; } = null!;
}