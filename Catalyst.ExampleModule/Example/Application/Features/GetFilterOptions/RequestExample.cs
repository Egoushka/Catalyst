using Swashbuckle.AspNetCore.Filters;

namespace Catalyst.ExampleModule.Example.Application.Features.GetFilterOptions;

public class RequestExample : IExamplesProvider<Request>
{
    public Request GetExamples()
    {
        return new Request(); // No parameters for this request
    }
}