# EK-Cs-Discord-jester

Documentation for Discord Client: https://discordnet.dev/guides/getting_started/first-bot.html


# 1 - Run Locally

Download And Install `dotnet` SDK 7 at `https://download.visualstudio.microsoft.com/download/pr/974313ac-3d89-4c51-a6e8-338d864cf907/6ed5d4933878cada1b194dd1084a7e12/dotnet-sdk-7.0.302-win-x64.exe
`

run `dotnet dev-certs https --trust` to install development certificates

## 1.1 - Environment variables

A few Tokens or secrets are requires so the application may start properly. There are a few alternatives to configure them.

See [Official Microsoft Documentation on using appsettings.json](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0)

### 1.1.1 - Secrets

Alternatively, most secrets can also be loaded from Azure Key Vault if a Key Vault URL is provided through `KeyVault__VaultUri`

A detailed description of required Secrets below:

- `Discord__Token` 
  - A Bot token provided by the [Discord Application Portal](https://discord.com/developers/applications/). 
  - If not provided, the bot will not log in.
- `Notion__Token`
  - A Notion token provided by the [Notion Integrations Portal](https://www.notion.so/my-integrations).
  - If not provided, the client will not log in.
- `KeyVault__VaultUri` 
  - The URI to the Key vault to fetch Secrets from, should they not be available locally. 
  - Note that underscores '_' are translated to Minus '-' for finding a secret name.

### 1.1.1 - Configurations

Other Configurations available:

- `KeyVault__UseBrowserCredentials` 
  - If set to true, will authenticate on Azure Key Vault using Interactive browser Authentication.
