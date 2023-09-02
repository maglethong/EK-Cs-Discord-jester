using Azure.Data.Tables;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EK.Discord.Common.Base.Configuration;
using EK.Discord.Server.Azure.Business;
using Microsoft.Extensions.Azure;

namespace EK.Discord.Server.Azure;

/// <summary>
///     Extension class for <see cref="IServiceCollection"/> for configuring <see cref="ISecretsManager"/>
/// </summary>
public static class AzureDependencyInjectionConfiguration {

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

        void LogError(string msg) {
            Console.Error.WriteLine(msg);
        }

        serviceCollection
            .AddAzureClients(azClientBuilder => {
                    bool useBrowserCredentials = configuration.GetValue<bool>("Azure:UseBrowserCredentials");
                    string? keyVaultUri = configuration.GetValue<string>("Azure:KeyVault:VaultUri");
                    string? tableUri = configuration.GetValue<string>("Azure:Tables:ServiceUri");

                    // Key Vault
                    if (!string.IsNullOrWhiteSpace(keyVaultUri)) {
                        azClientBuilder
                            .AddClient<SecretClient, SecretClientOptions>((options, cred, sp) => {
                                    ILogger logger = sp.GetService<ILogger<AzureKeyVaultSecretsManager>>()!;
                                    SecretClient ret = new SecretClient(new Uri(keyVaultUri), cred, options);
                                    logger.LogDebug("Successfully Added Azure Secrets Client");
                                    return ret;
                                }
                            );
                        serviceCollection.AddSingleton<ISecretsManager, AzureKeyVaultSecretsManager>();
                    } else {
                        LogError("Failed to connect to Azure key Vault. Missing 'Azure:KeyVault:VaultUri' section in configuration.");
                    }

                    // Table Storage
                    if (!string.IsNullOrWhiteSpace(tableUri)) {
                        azClientBuilder
                            .AddClient<TableServiceClient, TableClientOptions>((options, cred, sp) => {
                                    ILogger logger = sp.GetService<ILogger<TableServiceClient>>()!;
                                    TableServiceClient ret = new TableServiceClient(new Uri(tableUri), cred, options);
                                    logger.LogDebug("Successfully Added Azure Tables Client");
                                    return ret;
                                }
                            );
                    } else {
                        LogError("Failed to connect to Azure Tables. Missing 'Azure:Tables:ServiceUri' section in configuration.");
                    }

                    // Authentication
                    if (useBrowserCredentials) {
                        azClientBuilder.UseCredential(new InteractiveBrowserCredential());
                    } else {
                        azClientBuilder.UseCredential(new DefaultAzureCredential());
                    }
                }
            );

        return serviceCollection;
    }

}