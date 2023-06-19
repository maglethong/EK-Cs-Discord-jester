using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EK.Discord.Common.Base.Configuration;

/// <summary>
///     A fallback <see cref="ISecretsManager"/> that fetches secrets from <see cref="IConfiguration"/>
/// </summary>
public class FallbackSecretsManager : ISecretsManager {

    /// <summary>
    ///  Constructor
    /// </summary>
    public FallbackSecretsManager(IConfiguration configuration, ILogger<FallbackSecretsManager> logger) {
        Configuration = configuration;
        Logger = logger;
    }

    private IConfiguration Configuration { get; }

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