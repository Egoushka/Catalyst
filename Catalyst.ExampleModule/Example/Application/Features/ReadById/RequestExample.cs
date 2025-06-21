using Swashbuckle.AspNetCore.Filters;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadById;

public class RequestExample : IExamplesProvider<Request>
{
    public Request GetExamples()
    {
        return new Request
        {
            Id = default
        };
    }
}