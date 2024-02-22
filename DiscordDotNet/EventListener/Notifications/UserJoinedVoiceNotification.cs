using Discord.WebSocket;
using DiscordDotNet.Services;
using Lavalink4NET;
using MediatR;

namespace DiscordDotNet.EventListener.Notifications;

public class UserJoinedVoiceNotification : INotification
{

    public SocketUser User { get; }
    public SocketVoiceState OldState { get; }
    public SocketVoiceState NewState { get; }
    public LavalinkAudioService _service { get; }
    
    public UserJoinedVoiceNotification(SocketUser user, SocketVoiceState voiceState, SocketVoiceState voiceState2, LavalinkAudioService service)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        OldState = voiceState;
        NewState = voiceState2;
        _service = service;
    }
    
}