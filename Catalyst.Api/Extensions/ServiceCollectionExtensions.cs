using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Catalyst.Common.Extensions;
using Catalyst.Common.Services;
using Catalyst.Common.Settings;
using Catalyst.Core.Converters;
using Catalyst.Core.MediatR;
using Catalyst.Core.MediatR.Behaviours;
using Catalyst.ExampleModule;
using FluentValidation;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Swashbuckle.AspNetCore.Filters;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Catalyst.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Keep assembly discovery logic
    private static readonly List<Assembly> Assemblies = FindApplicationAssemblies();

    private static List<Assembly> FindApplicationAssemblies()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Catalyst.*.dll");
        referencedPaths.ToList().ForEach(path =>
            loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null && !loadedAssemblies.Contains(entryAssembly))
        {
            loadedAssemblies.Add(entryAssembly);
        }

        var result = loadedAssemblies.Distinct().Where(a => a.FullName != null && a.FullName.StartsWith("Catalyst")).ToList();
        
        return result;
    }

    // --- Core Services ---
    public static void AddApplicationCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        services.AddScoped<ICorrelationIdGenerator, GuidCorrelationIdGenerator>();
        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
    }

    public static void AddApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // This uses the Common extension method which handles binding and validation
        services.ConfigureOptions<BaseInfoOptions>(configuration);
        services.ConfigureOptions<DatabaseOptions>(configuration);
        services.ConfigureOptions<FeatureFlagsOptions>(configuration);
        services.ConfigureOptions<ResilienceOptions>(configuration);
        services.ConfigureOptions<RateLimitingOptions>(configuration);
        services.ConfigureOptions<OpenTelemetryOptions>(configuration);
        services.ConfigureOptions<ApplicationInsightsOptions>(configuration);
        services.ConfigureOptions<SerilogOptions>(configuration);
        services.ConfigureOptions<AuthOptions>(configuration);
    }

    public static void AddApplicationFeatureFlags(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFeatureManagement(configuration.GetSection("FeatureFlags"));
    }

    // --- Infrastructure & Cross-Cutting ---
    public static void AddApplicationResilience(this IServiceCollection services)
    {
        // Correct: Configure HttpClient using IServiceProvider within the delegate
        services.AddHttpClient("ResilientClient")
            .AddStandardResilienceHandler((options) => // Use the overload providing IServiceProvider (sp)
            {
                var sp = services.BuildServiceProvider();
                // Resolve options *inside* the delegate using the provided sp
                var resilienceOptions = sp.GetRequiredService<IOptions<ResilienceOptions>>().Value;
                var rateLimitingOptions =
                    sp.GetRequiredService<IOptions<RateLimitingOptions>>().Value; // Assuming needed here?

                // Configure resilience strategies
                // Example: Use rateLimitingOptions if relevant for this client's resilience
                // options.RateLimiter = new HttpRateLimiterStrategyOptions { ... };
                options.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = resilienceOptions.Timeout };
                options.Retry = new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = resilienceOptions.MaxRetries,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                };
                options.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = resilienceOptions.CircuitBreakerThreshold,
                    MinimumThroughput = 10, // Example
                    SamplingDuration = TimeSpan.FromSeconds(30), // Example
                    BreakDuration = TimeSpan.FromSeconds(30) // Example
                };
                options.AttemptTimeout = new HttpTimeoutStrategyOptions
                    { Timeout = TimeSpan.FromSeconds(10) }; // Example
            });
    }

    public static void AddApplicationOpenTelemetry(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

        var baseInfoOptions = sp.GetRequiredService<IOptions<BaseInfoOptions>>().Value;
        var openTelemetryOptions = sp.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;
        var environment = sp.GetRequiredService<IHostEnvironment>(); // Get environment
        services.AddOpenTelemetry()
            .ConfigureResource((resource) => // Correctly uses sp
            {
                resource.AddService(
                    serviceName: baseInfoOptions.AppName,
                    serviceVersion: baseInfoOptions.AppVersion);
            })
            .WithTracing((builder) =>
            {
                builder
                    .AddSource(baseInfoOptions.AppName) // Add custom activity source
                    .AddAspNetCoreInstrumentation(options => // Configure instrumentation
                    {
                        options.RecordException = true; // Record exceptions as events
                        // options.Filter = ... // Optional: Filter requests
                    })
                    .AddHttpClientInstrumentation();

                if (Uri.TryCreate(openTelemetryOptions.OtlpEndpoint, UriKind.Absolute, out var endpointUri))
                {
                    builder.AddOtlpExporter(options => options.Endpoint = endpointUri);
                }
                else if (!string.IsNullOrWhiteSpace(openTelemetryOptions.OtlpEndpoint))
                {
                    var logger = sp.GetRequiredService<ILogger<OpenTelemetryOptions>>();
                    logger.LogWarning("Invalid OTLP Endpoint configured: {OtlpEndpoint}",
                        openTelemetryOptions.OtlpEndpoint);
                }

                if (environment.IsDevelopment()) // Add console exporter only in dev
                {
                    builder.AddConsoleExporter();
                }
            })
            .WithMetrics((builder) => // Correctly uses sp
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // .AddRuntimeInstrumentation() // Add runtime metrics (GC, threads, etc.)
                    // .AddProcessInstrumentation() // Add process metrics (CPU, memory)
                    .AddMeter(baseInfoOptions.AppName); // Add custom meter source
                // .AddPrometheusExporter(); // Configure Prometheus endpoint scraping
            });
    }

    public static void AddApplicationSecurityHeaders(this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
    }

    public static void AddApplicationGdprCompliance(this IServiceCollection services)
    {
        services.AddCookiePolicy(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Lax; // Lax is generally safer default
            options.Secure = CookieSecurePolicy.Always; // Good default for HTTPS
            // options.CheckConsentNeeded = context => true; // Only if implementing consent logic
        });
    }

    public static void AddApplicationApplicationInsights(this IServiceCollection services)
    {
    }

    public static void AddApplicationCors(this IServiceCollection services, IHostEnvironment environment)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Local"))
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllForDev", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }
    }

    public static void AddApplicationHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddNpgSql((sp) => // Correct: Uses sp
                {
                    var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                    return databaseOptions.ConnectionStrings.DefaultConnection;
                },
                name: "postgres",
                failureStatus: HealthStatus.Degraded,
                tags: ["database", "infrastructure"],
                timeout: TimeSpan.FromSeconds(15)); // Slightly longer timeout maybe

        // // Add Prometheus publisher here
        // services.AddHealthChecksUI() // If using HealthChecks UI package
        //    .AddPrometheusHealthCheckPublisher(); // Add publisher
    }

    public static void AddApplicationMetrics(this IServiceCollection services)
    {
        // This method might be redundant now if OTel setup includes AddPrometheusExporter
        // services.AddMetrics(); // Base call often implicit
    }

    public static void AddApplicationDatabase(this IServiceCollection services) // Removed Environment, resolve inside
    {
        // services.AddDbContext<ApplicationSharedContext>((serviceProvider, options) =>
        // {
        //     // Resolve options and environment inside delegate
        //     var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        //     var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        //     var connectionString = databaseOptions.ConnectionStrings.DefaultConnection;
        //
        //     if (string.IsNullOrWhiteSpace(connectionString))
        //     {
        //         var logger = serviceProvider.GetRequiredService<ILogger<DatabaseOptions>>();
        //         logger.LogCritical("DefaultConnection string is missing or empty in configuration.");
        //         throw new InvalidOperationException("Database connection string is not configured.");
        //     }
        //
        //     options.UseNpgsql(connectionString, npgsqlOptions =>
        //     {
        //         npgsqlOptions.CommandTimeout(databaseOptions.Timeout);
        //         npgsqlOptions.EnableRetryOnFailure(
        //             maxRetryCount: 3, // TODO: Consider making these configurable via ResilienceOptions?
        //             maxRetryDelay: TimeSpan.FromSeconds(5),
        //             errorCodesToAdd: null);
        //         npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "shared"); // Example schema
        //     });
        //
        //     if (environment.IsDevelopment() || environment.IsEnvironment("Local"))
        //     {
        //         options.EnableSensitiveDataLogging().EnableDetailedErrors();
        //         // Configure logging level for EF Core specifically if needed
        //         options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter((category, level) =>
        //             category == DbLoggerCategory.Database.Command.Name && level >= LogLevel.Information)));
        //     }
        //     else
        //     {
        //         // Minimal logging in production
        //         options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter((category, level) =>
        //             level >= LogLevel.Warning)));
        //     }
        // });
    }

    public static void AddApplicationMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(Assemblies.ToArray());
            // Register pipeline behaviors in the desired execution order
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>)); // Log entry/exit
            cfg.AddOpenBehavior(typeof(CommandContextBehavior<,>)); // Add context
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); // Validate
            // cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>)); // Wrap handler in transaction (when implemented)
        });

        services.AddScoped<IMediatrContext, MediatrContext>();
    }

    public static void AddApplicationOutputCaching(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            // Set a default policy (e.g., no cache or short expiry)
            options.AddBasePolicy(policy => policy.NoCache()); // Require opt-in caching

            // Example policies (keep as is)
            options.AddPolicy("Products",
                policy => policy.Tag("products").SetVaryByQuery("page", "pageSize", "sortOrder")
                    .Expire(TimeSpan.FromMinutes(5)));
            options.AddPolicy("NoCacheAuth", policy => policy.SetVaryByHeader("Authorization").NoCache());
        });
    }

    private static void ConfigureApplicationJsonOptions(JsonSerializerOptions serializerOptions,
        IHostEnvironment environment)
    {
        serializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)); // Enum as camelCase string
        serializerOptions.Converters.Add(new FluentResultConverter()); // Custom converter
        serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.PropertyNameCaseInsensitive = true;
        serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        serializerOptions.WriteIndented = environment.IsDevelopment(); // Indent only in Development
    }

    public static void AddApplicationJsonOptions(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

        services.Configure<JsonOptions>((options) =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();
            ConfigureApplicationJsonOptions(options.SerializerOptions, environment);
        });

        services.AddControllers()
            .AddJsonOptions((options) =>
            {
                var environment = sp.GetRequiredService<IHostEnvironment>();
                ConfigureApplicationJsonOptions(options.JsonSerializerOptions, environment);
            });
    }

    public static void AddApplicationOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

            // Resolve BaseInfoOptions to get Title/Version dynamically
            // Note: This runs early, so options must be configured *before* AddSwaggerGen is called
            // Or use a ConfigureOptions approach if needed later in the pipeline
            using var
                scope = services.BuildServiceProvider()
                    .CreateScope(); // Temp scope - less ideal but common for Swagger setup
            var baseInfo = scope.ServiceProvider.GetRequiredService<IOptions<BaseInfoOptions>>().Value;

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = baseInfo.AppName, // Use configured name
                Version = baseInfo.AppVersion, // Use configured version
                Description = "API for managing Application functionalities."
                // Add Contact, License etc.
            });

            // options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            // {
            //     /* ... */
            // });
            // options.AddSecurityRequirement(new OpenApiSecurityRequirement
            // {
            //     /* ... */
            // });

            options.ExampleFilters();

            // Include XML comments (looks correct)
            var xmlFiles = Assemblies
                .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.GetName().Name}.xml"))
                .Where(File.Exists)
                .ToList();
            xmlFiles.ForEach(f => options.IncludeXmlComments(f, includeControllerXmlComments: true));

            // Tagging logic (looks reasonable)
            // options.TagActionsBy(api => { /* ... */ });
            options.DocInclusionPredicate((_, _) => true);

            options.AddServer(new OpenApiServer
            {
                Url = "https://sb-dev.application.ua/api", // <-- Change this to your API's real URL
                Description = "Development"
            });
        });

        services.AddSwaggerExamplesFromAssemblies(Assemblies.ToArray());
    }

    public static void AddApplicationRateLimiting(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        services.AddRateLimiter((options) =>
        {
            var rateLimitingOptions = sp.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partitionKey = context.User?.Identity?.Name ??
                                   context.Connection.RemoteIpAddress?.ToString() ?? "default_partition";
                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingOptions.PermitLimit,
                        Window = rateLimitingOptions.Window,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger =
                    context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<RateLimitingOptions>>(); // Resolve logger here
                logger.LogWarning("Rate limit rejected request for {Path}}",
                    context.HttpContext.Request.Path);
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests; // Ensure status code set
                // Consider adding Retry-After header based on lease metadata if available
                // if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)) { ... }
                // await context.HttpContext.Response.WriteAsync("Too many requests...", cancellationToken); // Optional body
            };
        });
    }

    public static void AddApplicationBackgroundServices(this IServiceCollection services)
    {
    }

    public static void AddApplicationModules(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

        var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        
        services.AddExampleModuleModule(databaseOptions);

    }

    public static void AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies(Assemblies, ServiceLifetime.Scoped,
            includeInternalTypes: true);
    }
}