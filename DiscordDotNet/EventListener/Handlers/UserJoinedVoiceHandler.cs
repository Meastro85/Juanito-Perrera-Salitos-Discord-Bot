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

        if (user.Id == 186206650363805697)
        {
            await currentChannel.ConnectAsync(true);
        }
        
    }
}