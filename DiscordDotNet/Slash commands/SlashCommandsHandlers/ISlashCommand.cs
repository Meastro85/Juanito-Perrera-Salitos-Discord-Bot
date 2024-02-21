using Discord;
using Discord.WebSocket;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public interface ISlashCommand
{
    
    SlashCommandProperties Build();
    Task Handle(SocketSlashCommand command);
    
}