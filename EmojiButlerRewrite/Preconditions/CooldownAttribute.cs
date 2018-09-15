using Discord.Commands;
using EmojiButlerRewrite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmojiButlerRewrite.Preconditions
{
    public class CooldownAttribute : PreconditionAttribute
    {
        public double CooldownInSeconds { get; set; }

        public CooldownAttribute(double cooldownInSeconds)
        {
            CooldownInSeconds = cooldownInSeconds;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var tracker = services.GetRequiredService<CooldownTrackerService>();

            if (tracker.TryGetSecondsElapsed(command.Name, context.User.Id, out var seconds))
            {
                if (seconds < CooldownInSeconds)
                {
                    return PreconditionResult.FromError($"You are on cooldown for this command. Seconds remaining: {Math.Round(CooldownInSeconds - seconds, 2)}s");
                }
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
