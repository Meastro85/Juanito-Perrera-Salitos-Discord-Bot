using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNet.Services;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public class AudioCommands : InteractionModuleBase<SocketInteractionContext>
{
    
    private readonly IAudioService _audioService;
    private readonly LavalinkAudioService _customService;

    public AudioCommands(IAudioService audioService, LavalinkAudioService service)
    {
        _audioService = audioService;
        _customService = service;
        _audioService.TrackEnded += TrackEndedAsync;
        _audioService.TrackStarted += TrackStartedAsync;
    }

    [SlashCommand("play", description: "Search and play the given song.", runMode: RunMode.Async)]
    public async Task Play(string query)
    {
        await DeferAsync().ConfigureAwait(false);

        var user = Context.Guild.GetUser(Context.User.Id);
        var voiceChannel = user.VoiceChannel;
        if (voiceChannel == null)
        {
            await FollowupAsync("You are not currently in a voice channel.").ConfigureAwait(false);
            return;

        }
        
        var player = await _customService.GetPlayerAsync(Context.Guild.Id, voiceChannel.Id).ConfigureAwait(false);

        if (player is null)
        {
            
            return;
        }

        var track = _customService.GetTrack(query).Result;

        if (track is null)
        {
            await FollowupAsync("No result.").ConfigureAwait(false);
            return;
        }

        await player.PlayAsync(track).ConfigureAwait(false);

        if (player.State == PlayerState.NotPlaying)
        {
            return;
        }
        await FollowupAsync($"Adding {track.Title} by {track.Author} to the queue.").ConfigureAwait(false);
    }

    private async Task TrackEndedAsync(object obj, TrackEndedEventArgs args)
    {
        await Task.CompletedTask;
    }

    private async Task TrackStartedAsync(object obj, TrackStartedEventArgs args)
    {
        if (Context is null)
        {
            return;
        }
        await FollowupAsync($"Now playing: {args.Track.Title} by {args.Track.Author}").ConfigureAwait(false);
    }

}