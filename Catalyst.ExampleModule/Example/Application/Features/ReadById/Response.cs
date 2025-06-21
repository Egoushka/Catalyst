using Catalyst.Common.Models;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadById;

public sealed record Response : BaseResponse<Response.Result>
{
    public sealed record Result : BaseResult
    {
        public required int Id { get; init; }

        public required int TypeId { get; init; }
        public required string TypeName { get; init; }

        public required string SerialNumber { get; init; }
        public required int GeneralStatusId { get; init; }

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

                GeneralStatusId = (int)exampleEntity.GeneralStatus,

                CreatedDateTime = exampleEntity.CreatedDateTime,
                UpdatedDateTime = exampleEntity.UpdatedDateTime,
            };
        }
    }
}