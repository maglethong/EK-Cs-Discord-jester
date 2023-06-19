using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EK.Discord.Server.Discord.Base;

/// <summary>
///     Extension class for <see cref="IServiceCollection"/> for configuring <see cref="IDiscordClient"/>
/// </summary>
public static class DiscordDependencyInjectionExtension {

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
    public static IServiceCollection AddDiscord(this IServiceCollection serviceCollection, IConfiguration configuration) {
        if (string.IsNullOrEmpty(configuration["Discord:Token"])) {
            Console.WriteLine("Failed to log in to Discord. Missing Token in configuration.");
            return serviceCollection;
        }

        serviceCollection.TryAddSingleton<IDiscordCommandHandler, DefaultDiscordCommandHandler>();
        return serviceCollection
               .AddDiscordClient(configuration["Discord:Token"]!)
               .AddDiscordCommandService(options => {
                       options.CaseSensitiveCommands = false;
                       options.IgnoreExtraArgs = false;
                       options.LogLevel = configuration.GetValue("Discord:LogLevel", LogSeverity.Info);
                   }
               );
    }

    private static IServiceCollection AddDiscordCommandService(this IServiceCollection serviceCollection, Action<CommandServiceConfig>? configuration) {
        return serviceCollection
            .AddSingleton<CommandService>(sp => {
                    CommandServiceConfig commandServiceConfiguration = new CommandServiceConfig();
                    configuration?.Invoke(commandServiceConfiguration);
                    CommandService commandService = new CommandService(commandServiceConfiguration);

                    // Add all Classes of current assembly that inherit from ModuleBase<SocketCommandContext>
                    commandService.AddModulesAsync(Assembly.GetEntryAssembly(), sp)
                                  .Wait();

                    return commandService;
                }
            );
    }


    private static IServiceCollection AddDiscordClient(this IServiceCollection serviceCollection, string token) {
        return serviceCollection
            .AddSingleton<IDiscordClient>(sp => {
                    DiscordSocketClient client = new();

                    client.LoginAsync(TokenType.Bot, token)
                          .Wait();

                    IDiscordCommandHandler commandHandler = sp.GetService<IDiscordCommandHandler>()!;
                    client.MessageReceived += message => commandHandler.HandleMessage(message);
                    return client;
                }
            );
    }

}