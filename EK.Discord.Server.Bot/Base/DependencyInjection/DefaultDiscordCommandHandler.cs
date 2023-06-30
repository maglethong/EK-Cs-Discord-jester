using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace EK.Discord.Server.Bot.Base.DependencyInjection;

/// <summary>
///     Default implementation of <see cref="IDiscordCommandHandler"/> as Suggested by https://discordnet.dev/guides/text_commands/intro.html
/// </summary>
public sealed class DefaultDiscordCommandHandler : IDiscordCommandHandler {

    public DefaultDiscordCommandHandler(IServiceProvider serviceProvider,
                                        CommandService commandService) {
        ServiceProvider = serviceProvider;
        CommandService = commandService;
    }

    private IServiceProvider ServiceProvider { get; }
    private IDiscordClient Client => ServiceProvider.GetService<IDiscordClient>()!;
    private CommandService CommandService { get; }

    /// <inheritdoc/>
    public async Task HandleMessage(SocketMessage message) {
        // Don't process the command if it was a system message
        SocketUserMessage? userMessage = message as SocketUserMessage;
        if (userMessage == null) {
            return;
        }

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if ((!userMessage.HasCharPrefix('!', ref argPos) && 
             !userMessage.HasMentionPrefix(Client.CurrentUser, ref argPos)) || 
            userMessage.Author.IsBot) {
            return;
        }

        // Create a WebSocket-based command context based on the message
        SocketCommandContext context = new SocketCommandContext((DiscordSocketClient) Client, userMessage);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await CommandService.ExecuteAsync(context: context,
                                          argPos: argPos,
                                          services: ServiceProvider
        );
    }

}