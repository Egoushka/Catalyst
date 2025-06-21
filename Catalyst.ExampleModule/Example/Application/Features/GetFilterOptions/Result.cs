using Catalyst.Common.Models;

namespace Catalyst.ExampleModule.Example.Application.Features.GetFilterOptions;

public class FilterOptionDto
{
    public object Value { get; set; } // Can be int or string
    public string Label { get; set; }
}

public record Result : BaseResult
{
    public IEnumerable<FilterOptionDto> Types { get; set; } = [];
}