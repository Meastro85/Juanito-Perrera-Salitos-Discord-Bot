using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordDotNet.EventListener;
using DiscordDotNet.Slash_commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static DiscordDotNet.Slash_commands.SlashCommandRegisterer;

namespace DiscordDotNet;

public class Program
{
    private static string _botToken;
    private static DiscordSocketClient _client;
    private static DiscordEventListener _listener;
    private static SlashCommandsListener _commandsListener;
    private static InteractionService _interactionService;
    private static IServiceProvider _serviceProvider;

    private static async Task Main()
    {
        DiscordSocketConfig config = new()
        {
            UseInteractionSnowflakeDate = false
        };
        using IHost host = Host.CreateDefaultBuilder().ConfigureServices((_, services) =>
                services.AddSingleton(_ => new DiscordSocketClient(config))
                    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>())
                    .AddScoped<DiscordEventListener>()
                    .AddScoped<SlashCommandsListener>())
            .Build();

        DotNetEnv.Env.TraversePath().Load();
        _botToken = DotNetEnv.Env.GetString("BOT-TOKEN");

        using IServiceScope serviceScope = host.Services.CreateScope();
        _serviceProvider = serviceScope.ServiceProvider;

        
        
        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        _client.Log += Log;
        _client.Ready += ClientReady;
        
        _listener = _serviceProvider.GetRequiredService<DiscordEventListener>();
        _commandsListener = _serviceProvider.GetRequiredService<SlashCommandsListener>();

        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();
        
        await _commandsListener.StartAsync();
        await _listener.StartAsync();

        await Task.Delay(-1);
    }
    
    private static Task ClientReady()
    {
        return RegisterCommands(_client);
    }
    
    private static Task Log(LogMessage msg)
    {
        string logPath = "../../../../Logs/";
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