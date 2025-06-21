using Catalyst.Common.Models;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadAll;

public record Result : BaseResult
{
    public required int Id { get; init; }
    public required int TypeId { get; init; }
    public required string TypeName { get; init; }

    public required string SerialNumber { get; init; }
    public required byte GeneralStatusId { get; init; }

    public required DateTime CreatedDateTime { get; init; }
    public required DateTime UpdatedDateTime { get; init; }

    public static Result FromEntity(Domain.Models.ExampleEntity exampleEntity)
    {
        return new Result
        {
            Id = exampleEntity.Id,

            TypeId = exampleEntity.TypeId,
            TypeName = exampleEntity.Type.Name,

            SerialNumber = exampleEntity.SerialNumber,

            GeneralStatusId = (byte)exampleEntity.GeneralStatus,

            CreatedDateTime = exampleEntity.CreatedDateTime,
            UpdatedDateTime = exampleEntity.UpdatedDateTime,
        };
    }
}