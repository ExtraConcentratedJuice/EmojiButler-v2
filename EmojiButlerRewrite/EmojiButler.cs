using Discord;
using Discord.WebSocket;
using EmojiButlerRewrite.Entities;
using EmojiButlerRewrite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmojiButlerRewrite
{
    public class EmojiButler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandHandlerService commandHandler;
        private readonly DiscordEmojiService discordEmoji;
        private readonly EmojiButlerConfiguration configuration;

        public EmojiButler(IServiceProvider services)
        {
            this.client = services.GetRequiredService<DiscordSocketClient>();
            this.discordEmoji = services.GetRequiredService<DiscordEmojiService>();
            this.commandHandler = services.GetRequiredService<CommandHandlerService>();
            this.configuration = services.GetRequiredService<EmojiButlerConfiguration>();
        }

        public async Task RunAsync()
        {
            client.Log += async (LogMessage x) => Console.WriteLine($"[{x.Severity}] {x.Message}");

            discordEmoji.Start();
            await commandHandler.InitializeAsync();

            await client.LoginAsync(TokenType.Bot, configuration.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
