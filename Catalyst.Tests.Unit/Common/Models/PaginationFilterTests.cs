using Catalyst.Common.Models;
using FluentAssertions;

namespace Catalyst.Tests.Unit.Common.Models;

public class PaginationFilterTests : BaseTest
{
    [Theory]
    [InlineData(1, 10, 0, 10)]
    [InlineData(2, 10, 10, 10)]
    [InlineData(3, 25, 50, 25)]
    [InlineData(1, 100, 0, 100)]
    public void Properties_SkipAndTake_ShouldCalculateCorrectly(int pageNumber, int pageSize, int expectedSkip, int expectedTake)
    {
        // Arrange & Act
        var filter = new PaginationFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Assert
        filter.Skip.Should().Be(expectedSkip);
        filter.Take.Should().Be(expectedTake);
    }
}