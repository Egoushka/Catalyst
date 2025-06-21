using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Keep for potential diagnostic use, but prefer ILogger long-term

namespace Catalyst.Common.Extensions;

public static class OptionsExtensions
{
    /// <summary>
    /// Configures an options class using the specified configuration section.
    /// It derives the section name from the options class name by removing the "Options" suffix.
    /// Includes data annotation validation and validation on application start.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options class.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureOptions<TOptions>(this IServiceCollection services,
        IConfiguration configuration)
        where TOptions : class
    {
        // Derive section name from class name (e.g., "DatabaseOptions" -> "Database")
        string sectionName = typeof(TOptions).Name.Replace("Options", string.Empty);
        IConfigurationSection section = configuration.GetSection(sectionName);

        if (!section.Exists())
        {
            // Use Debug.WriteLine for configuration-time diagnostics if needed,
            // but consider proper logging if this needs to be captured persistently.
            Debug.WriteLine(
                $"Warning: Configuration section '{sectionName}' not found for options '{typeof(TOptions).FullName}'. Binding default instance.");
        }
        else
        {
            Debug.WriteLine($"Info: Configuring '{typeof(TOptions).FullName}' from section '{sectionName}'.");
        }

        // Configure the options instance
        var optionsBuilder = services.AddOptions<TOptions>()
            .Bind(section); // Standard binding handles nested properties automatically

        // Add validation (requires Microsoft.Extensions.Options.DataAnnotations package)
        optionsBuilder.ValidateDataAnnotations() // Validates based on data annotations (e.g., [Required], [Range])
            .ValidateOnStart(); // Performs validation eagerly on application startup

        return services;
    }

    // Note: The GetOptions<T> extension method that builds the ServiceProvider prematurely
    // has been removed as it's generally considered an anti-pattern during configuration.
    // Options should be accessed via IOptions<T>, IOptionsSnapshot<T>, or IOptionsMonitor<T>
    // injected into services, or resolved from IServiceProvider within specific configuration delegates
    // like AddDbContext.
}