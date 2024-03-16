using CloudTranslationAPI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CloudTranslationAPI.TelegramBot;

class Program
{
    static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.AddSingleton<ChatStateMachine>();
        builder.Services.AddSingleton(p => new CloudTranslationClient(builder.Configuration["AuthorizationToken"]!, 
            builder.Configuration["ProjectId"]!));
        builder.Services.AddHostedService<CloudTranslationTelegramBot>(p => new CloudTranslationTelegramBot(
            builder.Configuration["TelegramBotToken"]!, p.GetRequiredService<ChatStateMachine>(),
             p.GetRequiredService<CloudTranslationClient>()));

        IHost host = builder.Build();
        host.Run();
    }
}