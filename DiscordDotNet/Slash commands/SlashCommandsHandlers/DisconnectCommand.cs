using Discord;
using Discord.WebSocket;

namespace DiscordDotNet.Slash_commands.SlashCommandsHandlers;

public class DisconnectCommand : ISlashCommand
{

    private DiscordSocketClient _client;
    
    public DisconnectCommand(){}
    public DisconnectCommand(DiscordSocketClient client)
    {
        _client = client;
    }
    
    public SlashCommandProperties Build()
    {
        return new SlashCommandBuilder
        {
            Name = "disconnect",
            Description = "Disconnect the bot from the call.",
            IsDefaultPermission = true
        }.Build();
    }
    
    public async Task Handle(SocketSlashCommand command)
    {
        Console.WriteLine("Disconnect command executed.");

        SocketGuild guild = ((SocketGuildChannel)(command.Channel)).Guild;
        SocketGuildUser bot = guild.CurrentUser;
        
        if (bot.VoiceChannel == null)
        {
            await command.RespondAsync("I'm not connected to a voice chat.");
            return;
        }
        
        ulong commandUserId = command.User.Id;
        SocketGuildUser user = guild.GetUser(commandUserId);
        
        if (user.VoiceChannel == null)
        {
            await command.RespondAsync("You're not connected to a voice chat.");
            return;
        }

        if (user.VoiceChannel != bot.VoiceChannel)
        {
            await command.RespondAsync("You're not in the same voice chat.");
            return;
        }

        if (user.VoiceChannel == bot.VoiceChannel)
        {
            await command.RespondAsync("Disconnecting...", ephemeral: false);
            await bot.VoiceChannel.DisconnectAsync();
        }
    }
}