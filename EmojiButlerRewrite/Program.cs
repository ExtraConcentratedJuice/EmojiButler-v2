using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmojiButlerRewrite.Entities;
using EmojiButlerRewrite.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;

namespace EmojiButlerRewrite
{
    class Program
    {
        static void Main(string[] args) => new EmojiButler(CreateServices()).RunAsync().GetAwaiter().GetResult();

        private static IServiceProvider CreateServices() =>
            new ServiceCollection()
            .AddSingleton(JsonConvert.DeserializeObject<EmojiButlerConfiguration>(File.ReadAllText("config.json")))
            .AddSingleton<DiscordEmojiService>()
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
#if DEBUG
                LogLevel = LogSeverity.Debug,
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
#else
                LogLevel = LogSeverity.Info
#endif
            }))
            .AddSingleton<ChoiceTrackerService>()
            .AddSingleton<CooldownTrackerService>()
            .AddSingleton<ReactionCollectorService>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlerService>()
            .BuildServiceProvider();
    }
}
