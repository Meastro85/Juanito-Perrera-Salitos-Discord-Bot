using DiscordDotNet.EventListener.Notifications;
using MediatR;

namespace DiscordDotNet.EventListener.Handlers;

public class GuildMessageReceivedHandler : INotificationHandler<GuildMessageReceivedNotification>
{
    public async Task Handle(GuildMessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.Message.Author.IsBot) return;
        // notification.Message.Channel.SendMessageAsync("Wllh");
    }
}