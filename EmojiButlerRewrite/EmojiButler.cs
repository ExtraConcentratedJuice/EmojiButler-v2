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

        public EmojiButler(DiscordSocketClient client, CommandHandlerService commandHandler, DiscordEmojiService discordEmoji, EmojiButlerConfiguration configuration)
        {
            this.client = client;
            this.discordEmoji = discordEmoji;
            this.commandHandler = commandHandler;
            this.configuration = configuration;
        }

        public async Task RunAsync()
        {
            client.Log += async (LogMessage x) => Console.WriteLine($"[{x.Severity}] {x.Message}");
            client.Ready += async () => await client.SetGameAsync("e:help | https://discordemoji.com", "https://twitch.tv/courierfive", ActivityType.Streaming);

            discordEmoji.Start();
            await commandHandler.InitializeAsync();

            await client.LoginAsync(TokenType.Bot, configuration.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
