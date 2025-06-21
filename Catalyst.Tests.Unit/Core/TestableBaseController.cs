using Catalyst.Common.Services;
using Catalyst.Core;
using Catalyst.Core.MediatR;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalyst.Tests.Unit.Core;

public class TestableBaseController : BaseController
{
    public TestableBaseController(IMediatrContext mediatrContext, ICorrelationIdProvider correlationIdProvider)
        : base(mediatrContext, correlationIdProvider)
    {
        // Mock HttpContext for ProblemDetails generation
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        // If CorrelationIdProvider.CorrelationId is accessed, ensure it's set
        correlationIdProvider.CorrelationId = Guid.NewGuid();
    }

    public IActionResult PublicHandleResult<T>(Result<T> result) => HandleResult(result); // Expose protected method
}