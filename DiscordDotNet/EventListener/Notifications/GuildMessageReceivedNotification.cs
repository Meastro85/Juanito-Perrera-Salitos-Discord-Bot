using Discord.WebSocket;
using MediatR;

namespace DiscordDotNet.EventListener.Notifications;

public class GuildMessageReceivedNotification : INotification
{

    public SocketMessage Message { get; }
    
    public GuildMessageReceivedNotification(SocketMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
    
}