namespace EK.Discord.Common.Base.Configuration;

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