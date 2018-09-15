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
        private readonly CooldownTrackerService cooldowns;
        private readonly Dictionary<ulong, DateTime> responseCooldowns = new Dictionary<ulong, DateTime>();

        public CommandHandlerService(DiscordSocketClient client, CommandService commands, EmojiButlerConfiguration configuration, CooldownTrackerService cooldowns, IServiceProvider serviceProvider)
        {
            this.client = client;
            this.commands = commands;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.cooldowns = cooldowns;
        }

        public async Task InitializeAsync()
        {
            await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceProvider);
            commands.Log += async (LogMessage x) => Console.WriteLine($"[{x.Severity}] {x.Message}");
            commands.CommandExecuted += OnCommandAsync;
            client.MessageReceived += OnMessageAsync;
        } 

        private async Task OnCommandAsync(CommandInfo info, ICommandContext context, IResult result)
        {
            if (result.IsSuccess)
                cooldowns.AddCooldown(info.Name, context.User.Id);
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
            {
                if (responseCooldowns.TryGetValue(context.User.Id, out var time))
                {
                    if ((DateTime.Now - time).Seconds < 3)
                        return;
                }

                if (result.Error.Value == CommandError.BadArgCount)
                {
                    await context.Channel.SendMessageAsync($"Invalid command usage. Check out ``{configuration.Prefix}help`` for info on how to use it.");
                }
                else if (result.Error.Value == CommandError.Exception)
                {
                    await context.Channel.SendMessageAsync($"Crap, something went wrong. You'd do me a favor by reporting this with ``{configuration.Prefix}reportissue <details>``.");
                }
                else if (result.Error.Value == CommandError.UnmetPrecondition)
                {
                    await context.Channel.SendMessageAsync($"You do not meet a precondition for this command: {result.ErrorReason}");
                }
                else
                {
                    await context.Channel.SendMessageAsync("Failed to execute: " + result.ErrorReason);
                }


                responseCooldowns[context.User.Id] = DateTime.Now; 
            }
        }
    }
}
