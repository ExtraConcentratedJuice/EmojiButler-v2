using Discord.Commands;
using EmojiButlerRewrite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmojiButlerRewrite.Preconditions
{
    public class RequireNoOpenChoiceAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var tracker = services.GetRequiredService<ChoiceTrackerService>();

            if (tracker.HasOpenChoice(context.User.Id))
                return PreconditionResult.FromError("You already have an open reaction choice, go take care of that first.");

            return PreconditionResult.FromSuccess();
        }
    }
}
