using Discord.Interactions.Builders;
using Discord.WebSocket;

namespace DiscordDotNet.Slash_commands;

public class SlashCommands
{
    private const ulong GuildId = 722451871779913739;

    public async Task RegisterCommands(DiscordSocketClient client)
    {
        var guild = client.GetGuild(GuildId);
        

    }
    
}