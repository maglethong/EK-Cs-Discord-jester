using Azure;
using Azure.Security.KeyVault.Secrets;
using EK.Discord.Common.Base.Configuration;

namespace EK.Discord.Server.Base.Configuration;

/// <summary>
///     A Secrets manager that uses <see cref="Azure"/>'s Key Vault for managing the secrets
/// </summary>
public class AzureKeyVaultSecretsManager : ISecretsManager {

    /// <summary>
    ///  Constructor
    /// </summary>
    public AzureKeyVaultSecretsManager(SecretClient secretClient, 
                                       FallbackSecretsManager fallbackSecretsManager, 
                                       ILogger<AzureKeyVaultSecretsManager> logger) {
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
            try {
                ret = SecretClient.GetSecret(secretName.Replace(":", "--")).Value.Value;
            } catch (RequestFailedException e) {
                Logger.LogError("Could connect to Azure Key Vault");
                Logger.LogTrace(e, "Could connect to Azure Key Vault");
            }
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