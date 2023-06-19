using EK.Discord.Common.Base.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notion.Client;

namespace EK.Discord.Server.Base.Notion; 

public static class NotionDependencyInjectionExtensions {

    public static IServiceCollection AddNotion(this IServiceCollection serviceCollection) {
        serviceCollection.AddSingleton<INotionClient>(sp => {
                             NotionClientLogging.ConfigureLogger(sp.GetService<ILoggerFactory>());
                             return NotionClientFactory.Create(sp.GetService<ClientOptions>());
                         })
                         .TryAddSingleton<ClientOptions>(sp => {
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
        
        return serviceCollection;
    }

}