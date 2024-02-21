using Discord;
using Discord.WebSocket;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public interface ISlashCommand
{
    
    Task Handle();
    
}