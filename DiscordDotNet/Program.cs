using System.Text;
using Discord;
using Discord.WebSocket;
using DiscordDotNet.EventListener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordDotNet;

public class Program
{
    private static  string _botToken;
    private static  DiscordSocketClient _client;
    private static  DiscordEventListener _listener;

    private static async Task Main()
    {
        
        using IHost host = Host.CreateDefaultBuilder().ConfigureServices((_, services) =>
                services.AddSingleton(_ => new DiscordSocketClient())
                    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>())
                    .AddScoped<DiscordEventListener>())
            .Build();
        
        DotNetEnv.Env.TraversePath().Load();
        _botToken = DotNetEnv.Env.GetString("BOT-TOKEN");
        
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        
        _client = provider.GetRequiredService<DiscordSocketClient>();
        _client.Log += Log;
        _client.SlashCommandExecuted += SlashCommandHandler;

        _listener = provider.GetRequiredService<DiscordEventListener>();
        
        await _client.LoginAsync(TokenType.Bot, _botToken);
        await _client.StartAsync();

        await _listener.StartAsync();
        
        await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        string logPath = $"../../../../Logs/{DateTime.Today.ToString("dd/MM/yyyy").Replace("/", "")}.log";
        Console.WriteLine(msg.ToString());
        
        if (!File.Exists(logPath)) File.Create(logPath);
        
        string content = File.ReadAllText(logPath);
        string newContent = content + $"{msg.ToString()}\n";
        
        File.WriteAllText(logPath, newContent, Encoding.UTF8);
        return Task.CompletedTask;
    }

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        
    }
    
}