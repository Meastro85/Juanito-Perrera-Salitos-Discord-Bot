using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNet.Services;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public class AudioCommands : InteractionModuleBase<SocketInteractionContext>
{
    
    private readonly IAudioService _audioService;
    private readonly LavalinkAudioService _customService;
    private readonly DiscordSocketClient _client;

    public AudioCommands(IAudioService audioService, LavalinkAudioService service, DiscordSocketClient client)
    {
        _audioService = audioService;
        _customService = service;
        _client = client;
        _audioService.TrackEnded += TrackEndedAsync;
        _audioService.TrackStarted += TrackStartedAsync;
        _client.ButtonExecuted += MyButtonHandler;
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
        
        var player = await RetrievePlayer().ConfigureAwait(false);

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
        await FollowupAsync($"Adding `{track.Title}` by `{track.Author}` to the queue.").ConfigureAwait(false);
    }

    [SlashCommand("stop", description: "Stop the player.", runMode: RunMode.Async)]
    public async Task Stop()
    {
        
        var player = await RetrievePlayer(false).ConfigureAwait(false);

        if (player is null) return;

        if (player.CurrentTrack is null)
        {
            await RespondAsync("Nothing playing!").ConfigureAwait(false);
            return;
        }

        await player.StopAsync().ConfigureAwait(false);
        await RespondAsync("Stopped playing.").ConfigureAwait(false);

    }
    
    public async Task Stop(SocketMessageComponent component)
    {
        
        var player = await RetrievePlayer((ulong)component.GuildId!, false).ConfigureAwait(false);

        if (player is null) return;

        if (player.CurrentTrack is null)
        {
            await component.RespondAsync("Nothing playing!").ConfigureAwait(false);
            return;
        }

        await player.StopAsync().ConfigureAwait(false);
        await component.RespondAsync("Stopped playing.").ConfigureAwait(false);

    }

    [SlashCommand("pause", description: "Pause the music.", runMode: RunMode.Async)]
    public async Task Pause()
    {
        

        var player = await RetrievePlayer(false).ConfigureAwait(false);
        if (player is null) return;

        if (player.State is PlayerState.Paused)
        {
            await RespondAsync("The music has already been paused.").ConfigureAwait(false);
            return;
        }
        
        await player.PauseAsync().ConfigureAwait(false);
        await RespondAsync("Paused the music.", components: GetComponents(player).Build() ).ConfigureAwait(false);

    }
    
    public async Task Pause(SocketMessageComponent component)
    {
        

        var player = await RetrievePlayer((ulong)component.GuildId!,false).ConfigureAwait(false);
        if (player is null) return;

        if (player.State is PlayerState.Paused)
        {
            await component.RespondAsync("The music has already been paused.").ConfigureAwait(false);
            return;
        }
        
        await player.PauseAsync().ConfigureAwait(false);
        await component.RespondAsync("Paused the music.", components: GetComponents(player).Build() ).ConfigureAwait(false);

    }

    [SlashCommand("resume", description: "Resume the music.", runMode: RunMode.Async)]
    public async Task Resume()
    {
        var player = await RetrievePlayer(false).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        if (player.State is not PlayerState.Paused)
        {
            await RespondAsync("Music is not paused.").ConfigureAwait(false);
            return;
        }

        
        
        await player.ResumeAsync().ConfigureAwait(false);
        await RespondAsync("Resuming music.", components: GetComponents(player).Build()).ConfigureAwait(false);
    }

    public async Task Resume(SocketMessageComponent component)
    {
        var player = await RetrievePlayer((ulong)component.GuildId!,false).ConfigureAwait(false);

        if (player is null)
        {
            return;
        }

        if (player.State is not PlayerState.Paused)
        {
            await component.RespondAsync("Music is not paused.").ConfigureAwait(false);
            return;
        }
        
        await player.ResumeAsync().ConfigureAwait(false);
        await component.RespondAsync("Resuming music.", components: GetComponents(player).Build()).ConfigureAwait(false);
    }
    
    [SlashCommand("skip", description: "Skip the current song", runMode: RunMode.Async)]
    public async Task Skip()
    {
        var player = await RetrievePlayer().ConfigureAwait(false);

        if (player is null) return;

        if (player.CurrentTrack is null)
        {
            await RespondAsync("Nothing is playing!").ConfigureAwait(false);
            return;
        }

        await player.SkipAsync().ConfigureAwait(false);

        var track = player.CurrentTrack;

        if (track is not null)
        {
            await RespondAsync($"Skipped song.", components: GetComponents(player).Build()).ConfigureAwait(false);
        }
        else
        {
            await RespondAsync("Skipped. Stopped playing, queue is empty.").ConfigureAwait(false);
        }

    }
    
    public async Task Skip(SocketMessageComponent component)
    {
        var player = await RetrievePlayer((ulong)component.GuildId!,connectToVoiceChannel: false).ConfigureAwait(false);

        if (player is null) return;

        if (player.CurrentTrack is null)
        {
            await component.RespondAsync("Nothing is playing!").ConfigureAwait(false);
            return;
        }

        await player.SkipAsync().ConfigureAwait(false);

        var track = player.CurrentTrack;

        if (track is not null)
        {
            await component.RespondAsync($"Skipped song.", components: GetComponents(player).Build()).ConfigureAwait(false);
        }
        else
        {
            await component.RespondAsync("Skipped. Stopped playing, queue is empty.").ConfigureAwait(false);
        }

    }

    private async Task<QueuedLavalinkPlayer?> RetrievePlayer(bool connectToVoiceChannel = true)
    {
        var user = Context.Guild.GetUser(Context.User.Id);
        var voiceChannel = user.VoiceChannel;
        var guild = Context.Guild;

        var player = await _customService.GetPlayerAsync(guild.Id, voiceChannel.Id, connectToVoiceChannel: connectToVoiceChannel);
        
        return player;
    }
    
    private async Task<QueuedLavalinkPlayer?> RetrievePlayer(ulong guildId, bool connectToVoiceChannel = true)
    {

        var player = await _customService.GetPlayerAsync(guildId, 0, connectToVoiceChannel: connectToVoiceChannel);
        
        return player;
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
        
        var builder = new ComponentBuilder().WithButton("Skip", "skipButton").WithButton("Stop","stopButton", ButtonStyle.Danger);
        if (args.Player.State == PlayerState.Paused)
        {
            builder.WithButton("Resume", "resumeButton", ButtonStyle.Success);
        }
        else
        {
            builder.WithButton("Pause", "pauseButton");
        }
        
        await  FollowupAsync($"Now playing: `{args.Track.Title}` by `{args.Track.Author}`", components: GetComponents(args.Player as QueuedLavalinkPlayer).Build()).ConfigureAwait(false);
    }

    private ComponentBuilder GetComponents(QueuedLavalinkPlayer? player)
    {
        var builder = new ComponentBuilder().WithButton("Skip", "skipButton").WithButton("Stop","stopButton", ButtonStyle.Danger);
        if (player is { State: PlayerState.Paused })
        {
            builder.WithButton("Resume", "resumeButton", ButtonStyle.Success);
        }
        else
        {
            builder.WithButton("Pause", "pauseButton");
        }

        return builder;
    }

    public async Task MyButtonHandler(SocketMessageComponent component)
    {
        
        switch (component.Data.CustomId)
        {
            case "stopButton":
                await Stop(component);
                break;
            case "pauseButton":
                await Pause(component);
                break;
            case "resumeButton":
                await Resume(component);
                break;
            case "skipButton":
                await Skip(component);
                break;
        }
    }
    
}