using Discord.Interactions;
using DiscordDotNet.EventListener.Notifications;
using DiscordDotNet.Services;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using MediatR;

namespace DiscordDotNet.EventListener.Handlers;

public class UserJoinedVoiceHandler : INotificationHandler<UserJoinedVoiceNotification>
{
    
    public Task Handle(UserJoinedVoiceNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var user = notification.User;
            var currentVoiceState = notification.NewState;
            var currentChannel = currentVoiceState.VoiceChannel;
            var guildId = currentChannel.Guild.Id;
            var service = notification._service;
        
            // This is a user specific implementation.
            DotNetEnv.Env.TraversePath().Load();
            if (user.Id == ulong.Parse(DotNetEnv.Env.GetString("USER_AZAN_MEME")))
            {
                var player = await service.GetPlayerAsync(guildId, currentChannel.Id).ConfigureAwait(false);

                var track = service.GetTrack("https://www.youtube.com/watch?v=r6itxCaAd2w").Result;

                if (player is null || track is null) return;
            
                await player.PlayAsync(track, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            }
        });
        return Task.CompletedTask;
    }
}