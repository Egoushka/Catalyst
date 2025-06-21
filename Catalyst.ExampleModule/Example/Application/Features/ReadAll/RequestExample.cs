using Swashbuckle.AspNetCore.Filters;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadAll;

public class RequestExample : IExamplesProvider<Request>
{
    public Request GetExamples()
    {
        return new Request
        {
            PageNumber = 1,
            PageSize = 25
        };
    }
}