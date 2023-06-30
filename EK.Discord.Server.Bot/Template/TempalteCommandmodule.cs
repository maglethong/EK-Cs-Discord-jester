using Discord.Commands;
using Discord.WebSocket;
using Notion.Client;

namespace EK.Discord.Server.Bot.Template;

/// <summary>
///     Template Command Module from https://discordnet.dev/guides/text_commands/intro.html
/// </summary>
public sealed class TemplateCommandModule : ModuleBase<SocketCommandContext> {
    
    // ~ping -> pong
    [Command("ping")]
    [Summary("Echoes a message.")]
    public Task PingAsync() {
        return ReplyAsync("pong");
    }

    // ~say hello world -> hello world
    [Command("say")]
    [Summary("Echoes a message.")]
    public Task SayAsync([Remainder] [Summary("The text to echo")] string echo) {
        return ReplyAsync(echo);
    }

    // ~sample square 20 -> 400
    [Command("square")]
    [Summary("Squares a number.")]
    public async Task SquareAsync([Summary("The number to square.")] int num) {
        // We can also access the channel from the Command Context.
        await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
    }

    // ~sample userinfo --> foxbot#0282
    // ~sample userinfo @Khionu --> Khionu#8708
    // ~sample userinfo Khionu#8708 --> Khionu#8708
    // ~sample userinfo Khionu --> Khionu#8708
    // ~sample userinfo 96642168176807936 --> Khionu#8708
    // ~sample whois 96642168176807936 --> Khionu#8708
    [Command("userinfo")]
    [Summary("Returns info about the current user, or the user parameter, if one passed.")]
    [Alias("user", "whois")]
    public async Task UserInfoAsync([Summary("The (optional) user to get info from")] SocketUser? user = null) {
        SocketUser? userInfo = user ?? Context.Client.CurrentUser;
        await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
    }

}

/// <summary>
///     Template Command Module from https://discordnet.dev/guides/text_commands/intro.html
/// </summary>
public sealed class NotionTemplateCommandModule : ModuleBase<SocketCommandContext> {


    public NotionTemplateCommandModule(INotionClient notionClient) {
        NotionClient = notionClient;
    }

    private INotionClient NotionClient { get; }


    [Command("Get-Ashlyn-Str-From-Notion")]
    [Summary("test")]
    [Alias("notion")]
    public async Task GetAshlynStr() {
        var properties = await NotionClient
                               .Databases
                               .QueryAsync("0b5a5f4136af417b90ed38383fe69312", new DatabasesQueryParameters());


        var str = properties.Results
                            .Find(o => ((RichTextText) ((TitlePropertyValue) o.Properties["Skill"]).Title[0])
                                       .PlainText
                                       .Equals("STR")
                            )!;

        var strVal = ((NumberPropertyValue) str.Properties["Saving Throw"]).Number!;
        
        await ReplyAsync($"{strVal}");
    }
}