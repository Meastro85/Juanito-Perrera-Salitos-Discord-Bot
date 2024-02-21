using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public class DisconnectCommand : InteractionModuleBase<SocketInteractionContext>, ISlashCommand 
{
    
    [SlashCommand("disconnect","Disconnect the bot from the call.")]
    public async Task Handle()
    {
        
        Console.WriteLine("Disconnect command executed.");

        SocketGuild guild = Context.Guild;
        SocketGuildUser bot = guild.CurrentUser;
        
        if (bot.VoiceChannel == null)
        {
            await RespondAsync("I'm not connected to a voice chat.");
            return;
        }
        
        ulong commandUserId = Context.User.Id;
        SocketGuildUser user = guild.GetUser(commandUserId);
        
        if (user.VoiceChannel == null)
        {
            await RespondAsync("You're not connected to a voice chat.");
            return;
        }

        if (user.VoiceChannel != bot.VoiceChannel)
        {
            await RespondAsync("You're not in the same voice chat.");
            return;
        }

        if (user.VoiceChannel == bot.VoiceChannel)
        {
            await RespondAsync("Disconnecting...", ephemeral: false);
            await bot.VoiceChannel.DisconnectAsync();
        }
    }
}