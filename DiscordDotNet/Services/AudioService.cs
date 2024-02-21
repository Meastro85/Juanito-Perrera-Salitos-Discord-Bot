using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using Sprache;

namespace DiscordDotNet.Services;

public class AudioService
{
    
    private readonly DiscordSocketClient _client;
    private readonly IAudioService _audioService;


    public AudioService(DiscordSocketClient client, IAudioService audioService)
    {
        _client = client;
        _audioService = audioService;
    }

    public async Task Play(SocketSlashCommand command)
    {
        await command.DeferAsync().ConfigureAwait(false);
        
        var player = await GetPlayerAsync(command);

        if (player is null)
        {
            return;
        }

        string query = command.Data.Options.First().Value.ToString() ?? "";
        
        var track = await _audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);

        if (track is null)
        {
            await command.FollowupAsync("No track found.").ConfigureAwait(false);
            return;
        }
        
        await player.PlayAsync(track).ConfigureAwait(false);
        await command.FollowupAsync($"Now playing {track.Title}.").ConfigureAwait(false);
    }
    
    private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(SocketSlashCommand command, bool connectToVoiceChannel = true)
    {
        var channelBehavior = connectToVoiceChannel
            ? PlayerChannelBehavior.Join
            : PlayerChannelBehavior.None;
        var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);
        var guildId = ((SocketGuildChannel)(command.Channel)).Guild.Id;
        
        var playerOptions = new QueuedLavalinkPlayerOptions
        {
            ClearHistoryOnStop = true,
            ClearQueueOnStop = true,
            DisconnectOnStop = true,
            DisconnectOnDestroy = true
        } as IOptions<QueuedLavalinkPlayerOptions>;
        
        var result = await _audioService.Players
            .RetrieveAsync(guildId, null, PlayerFactory.Queued, playerOptions , retrieveOptions)
            .ConfigureAwait(false);

        if (result.IsSuccess) return result.Player;
        
        var errorMessage = result.Status switch
        {
            PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
            PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
            _ => "Unknown error.",
        };
            
        await command.FollowupAsync(errorMessage).ConfigureAwait(false);
        return null;

    }

}