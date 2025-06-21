using Catalyst.Common.Middleware;
using Catalyst.Common.Services;
using Catalyst.Core.Serilog.Enrichers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;
using Serilog;

namespace Catalyst.Api.Extensions;

public static class ProgramHelper
{
    // --- Helper Methods for Organization ---

    public static IServiceCollection AddApplicationCoreServices(this IServiceCollection services)
    {
        Log.Information("Adding core services for the application...");
       
        services.AddApplicationCorrelationId();
        services.AddHttpContextAccessor(); // Needed by Correlation ID Provider
        
        return services;
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Information("Configuring application options...");

        services.AddApplicationConfiguration(configuration);
        services.AddApplicationFeatureFlags(configuration);
        
        return services;
    }

    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        Log.Information("Setting up application infrastructure services...");

        services.AddApplicationDatabase(); // Pass environment for logging config
        services.AddApplicationCors(environment);
        services.AddApplicationSecurityHeaders();
        services.AddApplicationGdprCompliance();
        services.AddApplicationResilience();
        // Add Authentication/Authorization services here if using AuthOptions
        // services.AddAuthentication(...);
        // services.AddAuthorization(...);
        
        return services;
    }

    public static IServiceCollection AddApplicationMonitoringAndObservability(this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Information("Configuring monitoring and observability services...");

        services.AddApplicationOpenTelemetry();
        services.AddApplicationApplicationInsights(); // Configures itself based on options
        services.AddApplicationHealthChecks();
        services.AddApplicationMetrics();
        
        return services;
    }

    public static IServiceCollection AddApplicationApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Information("Setting up application-specific services and modules...");

        services.AddApplicationMediatR();
        services.AddFluentValidation();
        services.AddApplicationOutputCaching();
        services.AddApplicationJsonOptions();
        services.AddApplicationOpenApi(); // Includes AddEndpointsApiExplorer
        services.AddApplicationRateLimiting();
        services.AddApplicationBackgroundServices();
        services.AddApplicationModules();
        services.AddControllers(); // Add MVC Controllers support
        
        return services;
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        Log.Information("Configuring middleware pipeline...");
        
        // --- Serilog Request Logging (early as possible) ---
        app.UseSerilogRequestLogging();

        // --- Correlation ID Middleware (very early) ---
        app.Use((context, next) => // Simple middleware to ensure Correlation ID is set
        {
            var provider = context.RequestServices.GetRequiredService<ICorrelationIdProvider>();
            if (!provider.CorrelationId.HasValue) // Set only if not already set (e.g., by incoming header)
            {
                var generator = context.RequestServices.GetRequiredService<ICorrelationIdGenerator>();
                provider.CorrelationId = generator.Generate();
            }

            // Ensure it's available for downstream logging via CorrelationIdMiddleware
            context.Items["CorrelationId"] = provider.CorrelationId; // Use HttpContext.Items
            return next(context);
        });
        app.UseMiddleware<CorrelationIdMiddleware>(); // Pushes CorrelationId to LogContext

        // --- Exception Handling / Development ---
        // if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
        // {
        app.UseDeveloperExceptionPage();
        // Add Swagger UI middleware
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapOpenApi(); // Map OpenAPI spec endpoint if needed separately
        // }
        // else
        // {
        //     // --- Production Error Handling ---
        //     // app.UseExceptionHandler("/Error"); // Redirect to error page (requires page)
        //     // Or use ProblemDetails middleware
        //     app.UseStatusCodePages(); // Handles non-success status codes without exceptions
        //     // app.UseHsts(); // Enable HSTS only in production (after confirming HTTPS setup)
        // }

        // --- Security & Common Middleware ---
        // app.UseHttpsRedirection(); // Enable only if HTTPS is properly configured and required
        app.UseRouting(); // Required for endpoint mapping
        app.UseCors(app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local")
            ? "AllowAllForDev"
            : "YourProductionPolicyName"); // Apply appropriate CORS policy
        app.UseCookiePolicy(); // Apply GDPR cookie policy
        app.UseAuthentication(); // Add BEFORE UseAuthorization
        app.UseAuthorization();
        app.UseOutputCache();
        app.UseRateLimiter();

        // --- Monitoring & Custom Logging ---
        app.UseHttpMetrics(); // Prometheus metrics
        app.UseMiddleware<RequestResponseLoggingMiddleware>(); // Your custom request logger
        
        Log.Information("Middleware pipeline configured successfully.");
    }

    public static void ConfigureEndpoints(WebApplication app)
    {
        app.MapControllers(); // Map attribute-routed controllers
        app.MapMetrics(); // Map Prometheus metrics endpoint (usually /metrics)
        app.MapHealthChecks("/health", new HealthCheckOptions // Customize health check endpoint
        {
            // Add custom response writer if needed
            // ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        Log.Information("Endpoints configured successfully.");
    }
}