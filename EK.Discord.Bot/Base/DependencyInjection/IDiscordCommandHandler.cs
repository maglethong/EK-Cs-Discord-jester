using Discord.WebSocket;

namespace EK.Discord.Bot.Base.DependencyInjection;

/// <summary>
///     Interface defining a Command Handler for <see cref="Discord"/>
/// </summary>
public interface IDiscordCommandHandler {
    /// <summary>
    ///     Handle a <see cref="SocketMessage"/> received from Discord by the client
    /// </summary>
    /// <param name="message"> The <see cref="SocketMessage"/> received </param>
    public Task HandleMessage(SocketMessage message);
}