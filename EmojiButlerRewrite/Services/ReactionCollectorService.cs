using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiButlerRewrite.Services
{
    public class ReactionCollectorService
    {
        private readonly DiscordSocketClient client;

        public ReactionCollectorService(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task<SocketReaction> GrabReaction(IUser user, IMessage message, Func<SocketReaction, bool> predicate, double timeout)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate), "Predicate cannot be null.");

            var completion = new TaskCompletionSource<SocketReaction>();

            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));

            cancellation.Token.Register(() => completion.TrySetResult(null));

            try
            {
                client.ReactionAdded += reactionChecker;
                client.MessageDeleted += deleteChecker;

                var res = await completion.Task.ConfigureAwait(false);
                return res;
            }
            finally
            {
                client.ReactionAdded -= reactionChecker;
                client.MessageDeleted -= deleteChecker;
            }

            async Task reactionChecker(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
            {
                if ((user == null || reaction.UserId == user.Id) && (message == null || reaction.MessageId == message.Id) && predicate(reaction))
                    completion.TrySetResult(reaction);
            }

            async Task deleteChecker(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
            {
                if (cache.Id == message.Id)
                    completion.TrySetResult(null);
            }
        }

        public async Task<SocketReaction> GrabReaction(IUser user, IMessage message, double timeout) => await GrabReaction(user, message, x => true, timeout);
    }
}
