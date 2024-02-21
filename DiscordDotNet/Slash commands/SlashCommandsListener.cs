using Discord.WebSocket;
using DiscordDotNet.Slash_commands.SlashCommandsHandlers;

namespace DiscordDotNet.Slash_commands;

public class SlashCommandsListener
{
    
    private readonly DiscordSocketClient _client;

    public SlashCommandsListener(DiscordSocketClient client)
    {
        _client = client;
    }

    public async Task StartAsync()
    {
        
        _client.SlashCommandExecuted += SlashCommandHandler;

        await Task.CompletedTask;
    }
    
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name.ToLower())
        {
            case "disconnect":
                await new DisconnectCommand(_client).Handle(command); break;
        }
    }
    
}