using Discord.WebSocket;
using DiscordDotNet.Slash_commands.SlashCommandsHandlers;

namespace DiscordDotNet.Slash_commands;

public class SlashCommandRegisterer
{

    public static async Task RegisterCommands(DiscordSocketClient client)
    {
        var guild = client.GetGuild(ulong.Parse(DotNetEnv.Env.GetString("TESTING-GUILD")));

        var commands = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(ISlashCommand).IsAssignableFrom(p) && p.IsClass);

        foreach (var command in commands)
        {
            var commandInstance = Activator.CreateInstance(command) as ISlashCommand;
            await guild.CreateApplicationCommandAsync(commandInstance.Build());
        }
        
    }
    
}