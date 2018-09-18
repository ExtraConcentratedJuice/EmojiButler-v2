using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmojiButlerRewrite.Entities;
using EmojiButlerRewrite.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EmojiButlerRewrite
{
    class Program
    {
        static async Task Main() => await ActivatorUtilities.CreateInstance<EmojiButler>(CreateServices()).RunAsync();

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
