using Discord.WebSocket;
using DiscordDotNet.Services;
using DiscordDotNet.Slash_commands.SlashCommandsHandlers;

namespace DiscordDotNet.Slash_commands;

public class SlashCommandsListener
{
    
    private readonly DiscordSocketClient _client;
    private readonly AudioService _audioService;

    public SlashCommandsListener(DiscordSocketClient client, AudioService audioService)
    {
        _client = client;
        _audioService = audioService;
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
            case "play":
                await new PlayCommand(_client, _audioService ).Handle(command); break;
        }
    }
    
}