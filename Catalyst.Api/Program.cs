using Catalyst.Api.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Serilog.Debugging.SelfLog.Enable(Console.Error);

// =================== TEMPORARY DIAGNOSTIC LOGGER ===================
// Replace your existing Log.Logger configuration with this.
// This logger will write DEBUG level logs to the console AND a file immediately.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // <-- CAPTURE EVERYTHING
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Silence noisy framework logs
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console() // Keep console output
    .WriteTo.File( // <-- ADD A ROLLING FILE SINK
        path: "Logs/diag-log-.txt", // It will create this folder and file
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();
// ===================================================================

Log.Information("--- STARTING DIAGNOSTIC RUN ---");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- 1. Configuration ---
    // NO CHANGES HERE
    builder.Configuration.Sources.Clear();
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange:true)
        .AddEnvironmentVariables();
    
    // --- 2. Logging ---
    builder.Services.AddSingleton(Log.Logger);
    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration) // This still reads levels, files, etc.
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console( // This adds/overrides the console sink with a theme
            theme: AnsiConsoleTheme.Code, // <-- THEME IS SET HERE, SAFELY
            // The outputTemplate will still be read from your JSON file!
            // This is an additive configuration.
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose // Ensure it captures everything from the config
        ));
    
    Log.Debug("--- Starting Service Registration ---");

    // --- 3. Service Registration ---
    Log.Debug("Registering: AddApplicationCoreServices...");
    builder.Services.AddApplicationCoreServices();
    
    Log.Debug("Registering: AddApplicationOptions...");
    builder.Services.AddApplicationOptions(builder.Configuration);

    Log.Debug("Registering: AddApplicationInfrastructure...");
    builder.Services.AddApplicationInfrastructure(builder.Configuration, builder.Environment);

    Log.Debug("Registering: AddApplicationMonitoringAndObservability...");
    builder.Services.AddApplicationMonitoringAndObservability(builder.Configuration);

    Log.Debug("Registering: AddApplicationApplication...");
    builder.Services.AddApplicationApplication(builder.Configuration);

    Log.Debug("--- Service Registration Complete ---");
    
    // --- 5. Build App ---
    Log.Debug("--- Building WebApplication ---");
    var app = builder.Build();
    Log.Debug("--- WebApplication Built Successfully ---");

    // --- 6. Middleware Pipeline Configuration ---
    ProgramHelper.ConfigurePipeline(app);
    ProgramHelper.ConfigureEndpoints(app);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "--- APPLICATION FAILED TO START ---"); // Use Fatal for startup errors
}
finally
{
    Log.Information("--- DIAGNOSTIC RUN COMPLETE. Shutting down. ---");
    Log.CloseAndFlush();
}