using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Options;
using Sprache;

namespace DiscordDotNet.Services;

public class LavalinkAudioService
{
    
    private readonly IAudioService _audioService;
    
    public LavalinkAudioService(IAudioService audioService)
    {
        _audioService = audioService;
    }
    
    public async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(ulong guildId, ulong voiceId, bool connectToVoiceChannel = true)
    {
        
        var channelBehavior = connectToVoiceChannel
            ? PlayerChannelBehavior.Join
            : PlayerChannelBehavior.None;

        var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);
        
        var result = await _audioService.Players
            .RetrieveAsync(guildId,
                voiceId,
                PlayerFactory.Queued,
                new OptionsWrapper<QueuedLavalinkPlayerOptions>(new QueuedLavalinkPlayerOptions()
                {
                    ClearHistoryOnStop = true,
                    ClearQueueOnStop = true,
                    DisconnectOnDestroy = true,
                    DisconnectOnStop = true
                }),
                retrieveOptions)
            .ConfigureAwait(false);

        if (result.IsSuccess) return result.Player;
        var errorMessage = result.Status switch
        {
            PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
            PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
            _ => "Unknown error."
        };

        await Program.Log(new LogMessage(LogSeverity.Error, "GetPlayerAsync", errorMessage));
        return null;

    }

    public async Task<LavalinkTrack?> GetTrack(string query)
    {
        var track = await _audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);
        return track;
    }

}