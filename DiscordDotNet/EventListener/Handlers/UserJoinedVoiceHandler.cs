using DiscordDotNet.EventListener.Notifications;
using MediatR;

namespace DiscordDotNet.EventListener.Handlers;

public class UserJoinedVoiceHandler : INotificationHandler<UserJoinedVoiceNotification>
{
    public async Task Handle(UserJoinedVoiceNotification notification, CancellationToken cancellationToken)
    {
        var user = notification.User;
        var currentVoiceState = notification.NewState;
        var currentChannel = currentVoiceState.VoiceChannel;

        // This is a user specific implementation.
        DotNetEnv.Env.TraversePath().Load();
        if (user.Id == ulong.Parse(DotNetEnv.Env.GetString("USER-AZAN-MEME")))
        {
            await currentChannel.ConnectAsync(true, external: true); 
        }
        
    }
}