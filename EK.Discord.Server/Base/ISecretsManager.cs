using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;

namespace EK.Discord.Server.Base;

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
                    }
                });
        }
        return serviceCollection;
    }

}

/// <summary>
///     A central class for managing Secrets such as API Keys, passwords or certificates
/// </summary>
public interface ISecretsManager {
    
    /// <summary>
    ///     Retrieve a Secret given it's name 
    /// </summary>
    /// <param name="secretName"> name of the secret to retrieve </param>
    /// <returns> The secret's value as a string </returns>
    public string GetSecret(string secretName);

    /// <inheritdoc cref="GetSecret"/>
    public Task<string> GetSecretAsync(string secretName);

}

/// <summary>
///     A Secrets manager that uses <see cref="Azure"/>'s Key Vault for managing the secrets
/// </summary>
public class AzureKeyVaultSecretsManager : ISecretsManager {

    /// <summary>
    ///  Constructor
    /// </summary>
    public AzureKeyVaultSecretsManager(SecretClient secretClient, FallbackSecretsManager fallbackSecretsManager, ILogger<AzureKeyVaultSecretsManager> logger) {
        SecretClient = secretClient;
        FallbackSecretsManager = fallbackSecretsManager;
        Logger = logger;
    }

    private SecretClient  SecretClient  { get; }
    private FallbackSecretsManager FallbackSecretsManager { get; }
    private ILogger<AzureKeyVaultSecretsManager> Logger { get; }


    /// <inheritdoc />
    public string GetSecret(string secretName) {
            string ret = FallbackSecretsManager.GetSecret(secretName);

            if (string.IsNullOrWhiteSpace(ret)) {
                ret = SecretClient.GetSecret(secretName.Replace("_", "-")).Value.Value;
            }
            
            if (string.IsNullOrWhiteSpace(ret)) {
                Logger.LogDebug("Could not find secret {SecretName} in Azure Key Vault", secretName);
            }

            return ret;
    }

    /// <inheritdoc />
    public async Task<string> GetSecretAsync(string secretName) {
        string ret = await FallbackSecretsManager.GetSecretAsync(secretName);

        if (string.IsNullOrWhiteSpace(ret)) {
            Response<KeyVaultSecret> response = await SecretClient.GetSecretAsync(secretName.Replace("_", "-"));
            ret = response.Value.Value;
        }
        
        if (string.IsNullOrWhiteSpace(ret)) {
            Logger.LogDebug("Could not find secret {SecretName} in Configuration", secretName);
        }

        return ret;
    }
}

// TODO
public class FallbackSecretsManager : ISecretsManager {

    /// <summary>
    ///  Constructor
    /// </summary>
    public FallbackSecretsManager(IConfiguration configuration, ILogger<FallbackSecretsManager> logger) {
        Configuration = configuration;
        Logger = logger;
    }

    private IConfiguration Configuration  { get; }

    private ILogger<FallbackSecretsManager> Logger { get; }

    /// <inheritdoc />
    public string GetSecret(string secretName) {
        string? ret = Configuration[secretName];
        if (string.IsNullOrWhiteSpace(ret)) {
            Logger.LogDebug("Could not find secret {SecretName} in Configuration", secretName);
        }
        return ret ?? string.Empty;
    }

    /// <inheritdoc />
    public Task<string> GetSecretAsync(string secretName) {
        return Task.FromResult(GetSecret(secretName));
    }
}