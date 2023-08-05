using EK.Discord.Common.Base.Configuration;
using EK.Discord.Server.Notion.Base.Api;
using EK.Discord.Server.Notion.Base.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Notion.Client;

namespace EK.Discord.Server.Notion.Base;

/// <summary>
///     Extension class for configuring Dependency injection of <see cref="INotionClient"/> and its required services.
/// </summary>
public static class NotionDependencyInjectionConfiguration {

    /// <summary>
    ///     Adds all required services for interacting with notion through <see cref="INotionClient"/> or <see cref="SimpleNotionCrudRepository{TEntity}"/>
    /// </summary>
    /// <remarks>
    ///     You still need to manually add your own implementation of the <see cref="SimpleNotionCrudRepository{TEntity}"/>.
    ///     Example below:
    /// <p/>
    ///     <code>
    ///         services.TryAddTransient&lt;IRepository&lt;MyEntity&gt;, NotionRepository&lt;MyEntity&gt;&gt;();
    ///     </code>
    /// </remarks>
    public static IServiceCollection AddNotion(this IServiceCollection services) {
        services.AddSingleton<INotionClient>(sp => {
                               NotionClientLogging.ConfigureLogger(sp.GetService<ILoggerFactory>());
                               return NotionClientFactory.Create(sp.GetService<ClientOptions>());
                           }
                       );
        services.TryAddSingleton<ClientOptions>(sp => {
                        string token = sp.GetService<ISecretsManager>()!
                                         .GetSecret("Notion:Token");
                        if (string.IsNullOrWhiteSpace(token)) {
                            sp.GetService<ILogger<INotionClient>>()!
                              .LogError("Failed to log in to Notion due to Missing Token");
                        }
                        return new ClientOptions() {
                            AuthToken = token
                        };
                    }
                );
        services.TryAddSingleton(typeof(INotionEntitySerializer<>), typeof(DefaultNotionEntitySerializer<>));
        return services;
    }
}