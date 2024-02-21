using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordDotNet.Services;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public class PlayCommand : ISlashCommand
{

    private DiscordSocketClient _client;
    private AudioService _audioService;

    
    public PlayCommand(){}
    public PlayCommand(DiscordSocketClient client, AudioService audioService)
    {
        _client = client;
        _audioService = audioService;
    }
    
    public SlashCommandProperties Build()
    {
        return new SlashCommandBuilder()
        {
            Name = "play",
            Description = "Plays a song.",
            IsDefaultPermission = true
        }
        .AddOption("url", ApplicationCommandOptionType.String, "The URL or name of the song.", isRequired: true)
            .Build();
    }

    public async Task Handle(SocketSlashCommand command)
    {
        await _audioService.Play(command);
    }
    
}