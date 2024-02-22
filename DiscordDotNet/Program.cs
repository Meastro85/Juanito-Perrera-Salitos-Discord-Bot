using System.Text;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNet.EventListener;
using DiscordDotNet.Services;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordDotNet;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        
        DotNetEnv.Env.TraversePath().Load();
        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
                services
                    .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
                    {
                        UseInteractionSnowflakeDate = false,
                        GatewayIntents = GatewayIntents.AllUnprivileged,
                        AlwaysDownloadUsers = true
                    }))
                    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                    .AddSingleton<InteractionHandler>()
                    .AddSingleton(_ => new CommandService())
                    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>())
                    .AddScoped<DiscordEventListener>()
                    .AddScoped<LavalinkAudioService>()
                    .AddLavalink()
                    .ConfigureLavalink(options =>
                    {
                        options.Passphrase = DotNetEnv.Env.GetString("LAVALINK_PASS");
                    }))
            .Build();
        
        await RunAsync(host);
    }

    private async Task RunAsync(IHost host)
    {

        DotNetEnv.Env.TraversePath().Load();
        
        using IServiceScope serviceScope = host.Services.CreateScope();
        var serviceProvider = serviceScope.ServiceProvider;


        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        var slashCommands = serviceProvider.GetRequiredService<InteractionService>();
        await serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();
        await serviceProvider.GetRequiredService<IAudioService>().StartAsync();
        
        client.Log += Log;
        client.Ready += async () =>
        {
            await slashCommands.RegisterCommandsToGuildAsync(ulong.Parse(DotNetEnv.Env.GetString("TESTING_GUILD")));
        };
        slashCommands.Log += Log;
        var eventListener = serviceProvider.GetRequiredService<DiscordEventListener>();

        await client.LoginAsync(TokenType.Bot, DotNetEnv.Env.GetString("BOT_TOKEN"));
        await client.StartAsync();

        await eventListener.StartAsync();

        await Task.Delay(-1);
    }

    public static Task Log(LogMessage msg)
    {
        string logPath = "../../../../../Logs/";
        string logFile = $"{DateTime.Today.ToString("dd/MM/yyyy").Replace("/", "")}.log";
        Console.WriteLine(msg.ToString());

        if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
        if (!File.Exists(logPath)) File.Create(logPath + logFile);

        string content = File.ReadAllText(logPath + logFile);
        string newContent = content + $"{msg.ToString()}\n";

        File.WriteAllText(logPath + logFile, newContent, Encoding.UTF8);
        return Task.CompletedTask;
    }
}