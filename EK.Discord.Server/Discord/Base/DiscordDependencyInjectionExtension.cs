using Discord;
using Discord.Commands;
using Discord.WebSocket;

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

        DiscordSocketClient client = new();

        client.LoginAsync(TokenType.Bot, configuration["Discord:Token"])
              .Wait();

        serviceCollection.AddSingleton<IDiscordClient>(client);

//        client.MessageReceived += o => HandleCommandAsync(client, null, o);

        return serviceCollection;
    }


    private static async Task HandleCommandAsync(DiscordSocketClient client,
                                                 CommandService commandService,
                                                 SocketMessage messageParam) {
        // Don't process the command if it was a system message
        SocketUserMessage? message = messageParam as SocketUserMessage;
        if (message == null) {
            return;
        }

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot) {
            return;
        }

        // Create a WebSocket-based command context based on the message
        SocketCommandContext context = new SocketCommandContext(client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await commandService.ExecuteAsync(context: context,
                                          argPos: argPos,
                                          services: null
        );
    }

}