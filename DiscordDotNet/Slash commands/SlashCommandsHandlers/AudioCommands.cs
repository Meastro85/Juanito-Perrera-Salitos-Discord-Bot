using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public class AudioCommands : InteractionModuleBase<SocketInteractionContext>
{
    
    private readonly IAudioService _audioService;

    public AudioCommands(IAudioService audioService)
    {
        _audioService = audioService;
        _audioService.TrackEnded += TrackEndedAsync;
    }

    [SlashCommand("play", description: "Search and play the given song.", runMode: RunMode.Async)]
    public async Task Play(string query)
    {
        await DeferAsync().ConfigureAwait(false);

        var player = await GetPlayerAsync().ConfigureAwait(false);

        if (player is null)
        {
            return;
        }
 
        var track = await _audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube)
            .ConfigureAwait(false);

        if (track is null)
        {
            await FollowupAsync("No result.").ConfigureAwait(false);
            return;
        }

        await player.PlayAsync(track).ConfigureAwait(false);
        if (player.State == PlayerState.Playing)
        {
            await FollowupAsync($"Added {track.Title} to the queue.").ConfigureAwait(false);
        }
        else
        {
            await FollowupAsync($"Now playing: {track.Title}").ConfigureAwait(false);
        }
    }
    
    private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
    {
        
        var channelBehavior = connectToVoiceChannel
            ? PlayerChannelBehavior.Join
            : PlayerChannelBehavior.None;

        var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);

        var user = Context.Guild.GetUser(Context.User.Id);
        var voiceChannel = user.VoiceChannel;
        var result = await _audioService.Players
            .RetrieveAsync(Context.Guild.Id,
                voiceChannel.Id,
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

        await FollowupAsync(errorMessage).ConfigureAwait(false);
        return null;

    }

    private async Task TrackEndedAsync(object obj, TrackEndedEventArgs args)
    {
        
    }
    
    
    
}