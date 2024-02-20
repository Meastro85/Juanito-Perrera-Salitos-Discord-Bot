using Discord.WebSocket;
using MediatR;

namespace DiscordDotNet.EventListener.Notifications;

public class UserJoinedVoiceNotification : INotification
{

    public SocketUser User { get; }
    public SocketVoiceState OldState { get; }
    public SocketVoiceState NewState { get; }
    
    public UserJoinedVoiceNotification(SocketUser user, SocketVoiceState voiceState, SocketVoiceState voiceState2)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        OldState = voiceState;
        NewState = voiceState2;
    }
    
}