using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EK.Discord.Common.Base.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace EK.Discord.Bot.Base.DependencyInjection;

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
        serviceCollection.TryAddSingleton<IDiscordCommandHandler, DefaultDiscordCommandHandler>();
        return serviceCollection
               .AddDiscordClient(options => {
                   // TODO -> Fix, unable to read commands not mentioning bot
//                                     options.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent;
                                     options.LogLevel = configuration.GetValue("Discord:LogLevel", LogSeverity.Debug);
                                 })
               .AddDiscordCommandService(options => {
                       options.CaseSensitiveCommands = false;
                       options.IgnoreExtraArgs = false;
                       options.LogLevel = configuration.GetValue("Discord:LogLevel", LogSeverity.Debug);
                   }
               );
    }

    private static IServiceCollection AddDiscordCommandService(this IServiceCollection serviceCollection, Action<CommandServiceConfig>? configuration = null) {
        return serviceCollection
            .AddSingleton<CommandService>(sp => {
                    CommandServiceConfig commandServiceConfiguration = new CommandServiceConfig();
                    configuration?.Invoke(commandServiceConfiguration);
                    CommandService commandService = new CommandService(commandServiceConfiguration);

                    // Add all Classes of current assembly that inherit from ModuleBase<SocketCommandContext>
                    commandService.AddModulesAsync(Assembly.GetEntryAssembly(), sp)
                                  .Wait();
                    if (Assembly.GetEntryAssembly() != Assembly.GetExecutingAssembly()) {
                        commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), sp)
                                      .Wait();
                    }

                    return commandService;
                }
            );
    }


    private static IServiceCollection AddDiscordClient(this IServiceCollection serviceCollection, 
                                                       Action<DiscordSocketConfig>? configuration = null) {
        return serviceCollection
            .AddSingleton<IDiscordClient>(sp => {
                    DiscordSocketConfig config = new DiscordSocketConfig();
                    configuration?.Invoke(config);
                    DiscordSocketClient client = new DiscordSocketClient(config);

                    IDiscordCommandHandler commandHandler = sp.GetService<IDiscordCommandHandler>()!;
                    client.MessageReceived += message => commandHandler.HandleMessage(message);
                    client.Log += message => {
                        LogLevel level = message.Severity switch {
                            LogSeverity.Critical => LogLevel.Critical,
                            LogSeverity.Info => LogLevel.Information,
                            LogSeverity.Debug => LogLevel.Debug,
                            LogSeverity.Error => LogLevel.Error,
                            LogSeverity.Warning => LogLevel.Warning,
                            LogSeverity.Verbose => LogLevel.Trace,
                            _ => throw new NotImplementedException($"Not implemented for {message.Severity}")
                        };
                        sp.GetService<ILogger<IDiscordClient>>()!.Log(level, "{}", message.Message);
                        return Task.CompletedTask;
                    };

                    string token = sp.GetService<ISecretsManager>()!
                                     .GetSecret("Discord:Token");

                    if (string.IsNullOrEmpty(token)) {
                        sp.GetService<ILogger<IDiscordClient>>()!
                          .LogError("Failed to log in to Discord due to Missing Token");
                    } else {
                        client.LoginAsync(TokenType.Bot, token)
                              .Wait();
                    }
                    
                    return client;
                }
            );
    }

}