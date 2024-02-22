using Discord.WebSocket;
using DiscordDotNet.EventListener.Notifications;
using DiscordDotNet.Services;
using Lavalink4NET;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordDotNet.EventListener;

public class DiscordEventListener
{
    private readonly CancellationToken _cancellationToken;
    private readonly DiscordSocketClient _client;
    private readonly IServiceScopeFactory _serviceScope;

    public DiscordEventListener(DiscordSocketClient client, IServiceScopeFactory serviceScope)
    {
        _client = client;
        _serviceScope = serviceScope;
        _cancellationToken = new CancellationTokenSource().Token;
    }

    private IMediator Mediator
    {
        get
        {
            var scope = _serviceScope.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMediator>();
        }
    }

    public async Task StartAsync()
    {
        
        _client.UserVoiceStateUpdated += OnUserJoinedVoiceAsync;
        _client.MessageReceived += OnGuildMessageReceivedAsync;
        
        await Task.CompletedTask;
    }

    private Task OnUserJoinedVoiceAsync(SocketUser user, SocketVoiceState voiceState, SocketVoiceState voiceState2)
    {
        if (voiceState2.VoiceChannel != null && !user.IsBot)
        {
            return Mediator.Publish(new UserJoinedVoiceNotification(user,
                voiceState,
                voiceState2,
                _serviceScope.CreateScope().ServiceProvider.GetRequiredService<LavalinkAudioService>()), _cancellationToken);
        }

        return Task.CompletedTask;

    }

    private Task OnGuildMessageReceivedAsync(SocketMessage message)
    {
        return Mediator.Publish(new GuildMessageReceivedNotification(message), _cancellationToken);
    }
    
}