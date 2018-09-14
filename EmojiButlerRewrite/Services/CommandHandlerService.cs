using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EmojiButlerRewrite.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmojiButlerRewrite.Services
{
    public class CommandHandlerService
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider serviceProvider;
        private readonly EmojiButlerConfiguration configuration;

        public CommandHandlerService(DiscordSocketClient client, CommandService commands, EmojiButlerConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.client = client;
            this.commands = commands;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceProvider);
            commands.Log += async (LogMessage x) => Console.WriteLine($"[{x.Severity}] {x.Message}");
            client.MessageReceived += OnMessageAsync;
        } 

        private async Task OnMessageAsync(SocketMessage msg)
        {
            if (!(msg is SocketUserMessage message) || message.Author.IsBot)
                return;

            var pos = 0;

            if (!message.HasStringPrefix(configuration.Prefix, ref pos, StringComparison.OrdinalIgnoreCase))
                return;

            var context = new SocketCommandContext(client, message);

            var result = await commands.ExecuteAsync(context, pos, serviceProvider);

            if (result.Error.HasValue && result.Error.Value != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ToString());
        }
    }
}
