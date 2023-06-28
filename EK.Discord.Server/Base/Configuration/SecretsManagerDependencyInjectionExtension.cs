using Azure.Identity;
using EK.Discord.Common.Base.Configuration;
using Microsoft.Extensions.Azure;

namespace EK.Discord.Server.Base.Configuration;

/// <summary>
///     Extension class for <see cref="IServiceCollection"/> for configuring <see cref="ISecretsManager"/>
/// </summary>
public static class SecretsManagerDependencyInjectionExtension {

    /// <summary>
    ///     Add Discord configuration as Singleton to Dependency injection.
    /// <para/>
    ///     NOTE: The client is not created if no Token cold be read from the configuration
    /// </summary>
    /// <remarks>
    ///     This log in the bot and keeps him logged in until the application closes
    /// </remarks>
    /// <param name="serviceCollection"> Class being extended </param>
    /// <param name="configuration"> A Configuration provider for reading <code> Discord:Token </code> </param>
    /// <returns> Class being extended </returns>
    public static IServiceCollection AddAzureKeyVaultSecretsManager(this IServiceCollection serviceCollection, IConfiguration configuration) {
        serviceCollection.AddSingleton<FallbackSecretsManager>()
                         .AddSingleton<ISecretsManager>(sp => sp.GetService<FallbackSecretsManager>()!);
        
        IConfigurationSection configurationSection = configuration.GetSection("KeyVault");
        if (!configurationSection.AsEnumerable().Any()) {
            Console.WriteLine("Failed to connect to Azure key Vault. Missing 'KeyVault' section in configuration.");
        } else {
            serviceCollection
                .AddSingleton<ISecretsManager, AzureKeyVaultSecretsManager>()
                .AddAzureClients(azClientBuilder => {
                    azClientBuilder.AddSecretClient(configurationSection);
                    if (configurationSection.GetValue<bool>("UseBrowserCredentials")) {
                        azClientBuilder.UseCredential(new InteractiveBrowserCredential());
                    } else {
                        azClientBuilder.UseCredential(new DefaultAzureCredential());
                    }
                });
        }
        return serviceCollection;
    }

}