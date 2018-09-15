using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using EmojiButlerRewrite.Entities;
using EmojiButlerRewrite.Preconditions;
using EmojiButlerRewrite.Services;

namespace EmojiButlerRewrite.Modules
{
    public class EmojiModule : ModuleBase
    {
        public DiscordEmojiService DiscordEmoji { get; set; }
        public ReactionCollectorService ReactionCollector { get; set; }
        public ChoiceTrackerService ChoiceTracker { get; set; }
        public EmojiButlerConfiguration Configuration { get; set; }

        private static readonly Emoji reactionYes = new Emoji("✅");
        private static readonly Emoji reactionNo = new Emoji("❌");

        [Command("addemoji", RunMode = RunMode.Async)]
        [Summary("Add an emoji to your server from DiscordEmoji.")]
        [Cooldown(3)]
        [RequireBotPermission(GuildPermission.ManageEmojis)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        [RequireBotPermission(GuildPermission.EmbedLinks)]
        [RequireContext(ContextType.Guild)]
        [RequireNoOpenChoice]
        public async Task AddEmoji(string name, string nameOverride = null)
        {
            var emoji = DiscordEmoji.EmoteFromName(name);

            if (emoji == null)
            {
                await ReplyAsync("No emoji by that name was found on DiscordEmoji." +
                    "\nPlease select a valid emoji from the catalog at https://discordemoji.com" +
                    "\n\n(The emoji name is case sensitive. Don't include the colons in your command!)" +
                    "\n\n If you're too lazy to go on the website, you can use the ``emojis`` command to list emojis." +
                    $"\n``{Configuration.Prefix}emojis <category> <page (Optional)>``");

                return;
            }

            if ((Context.Channel as ITextChannel).IsNsfw && DiscordEmoji.GetCategoryName(emoji.Category) == "NSFW")
            {
                await ReplyAsync("That's an NSFW emoji, go into an NSFW channel for that.");
                return;
            }

            var guildEmoji = Context.Guild.Emotes;

            if (guildEmoji.Count(x => x.Animated) >= 50 && emoji.Animated)
            {
                await ReplyAsync("You already have fifty (50) animated emoji on this server. That is Discord's limit. Remove some before adding more.");
                return;
            }

            if (guildEmoji.Count(x => !x.Animated) >= 50 && !emoji.Animated)
            {
                await ReplyAsync("You already have fifty (50) emoji on this server. That is Discord's limit. Remove some before adding more.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = "Confirmation",
                Description = "Are you sure that you want to add this emoji to your server?\nReact in less than 20 seconds to confirm.",
                ThumbnailUrl = emoji.Image
            };

            embed.AddField("Name", emoji.Title);

            if (nameOverride != null)
                embed.AddField("Name Override", nameOverride);

            embed.AddField("Author", emoji.Author);


            SocketReaction reaction;

            try
            {
                var sent = await ReplyAsync(embed: embed.Build());

                ChoiceTracker.AddUser(Context.User.Id);

                await sent.AddReactionAsync(reactionYes);
                await sent.AddReactionAsync(reactionNo);

                reaction = await ReactionCollector.GrabReaction(Context.User, sent,
                    x => x.Emote.Equals(reactionYes) || x.Emote.Equals(reactionNo), 20);
            }
            finally
            {
                ChoiceTracker.RemoveUser(Context.User.Id);
            }


            if (reaction == null)
            {
                await ReplyAsync("No reaction was given, aborting.");
                return;
            }

            if (reaction.Emote.Equals(reactionYes))
            {
                var successMessage = await ReplyAsync("Hold on while I add your emoji...");

                try
                {
                    using (Stream image = await DiscordEmoji.GetImageAsync(emoji))
                    {
                        // Check for Discord's 256kb emoji size cap
                        if (image.Length / 1024 > 256)
                        {
                            await ReplyAsync("The selected emoji is above 256kb and cannot be uploaded to Discord. Go bother Kohai (DiscordEmoji developer) to fix it.");
                            return;
                        }

                        await Context.Guild.CreateEmoteAsync(nameOverride ?? emoji.Title, new Image(image), options: new RequestOptions
                        {
                            AuditLogReason = $"Emoji added by {Context.User.Username}#{Context.User.Discriminator}",
                            RetryMode = RetryMode.AlwaysFail // Will fail on too long of a ratelimit so the bot doesn't hang on emoji upload ratelimit
                        });
                    }

                    var addMessage = $"Cool beans, you've added {emoji.Title} to your server.";

                    if (nameOverride != null)
                        addMessage += $" Name override: {nameOverride}";

                    await successMessage.ModifyAsync(x => x.Content = addMessage);
                }
                catch (Exception ex)
                {
                    if (ex is RateLimitedException)
                    {
                        await ReplyAsync("Discord has a ratelimit on adding emoji, and you've just hit it. Try adding your emoji at a later time.");
                        return;
                    }
                    else if (ex is HttpException hex)
                    {
                        string message;

                        switch (hex.HttpCode)
                        {
                            case (HttpStatusCode)400:
                                message = $"The emoji failed to submit to Discord, it may have been too big. {hex.Reason ?? ""}";
                                break;
                            default:
                                message = "Unexpected status code received from Discord, something went wrong. Report it, I guess?";
                                break;
                        }

                        await ReplyAsync(message);
                        return;
                    }
                }
            }
            else
            {
                await ReplyAsync("Alright then, I won't be adding that emoji.");
            }
        }

        [Command("removeemoji", RunMode = RunMode.Async)]
        [Summary("Remove an emoji from your server.")]
        [Cooldown(3)]
        [RequireBotPermission(GuildPermission.ManageEmojis)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        [RequireContext(ContextType.Guild)]
        [RequireNoOpenChoice]
        public async Task RemoveEmoji(string name)
        {
            var serverEmoji = Context.Guild.Emotes;

            var toRemove = serverEmoji.FirstOrDefault(x => x.Name == name);

            if (toRemove == null)
            {
                await ReplyAsync("No emoji was found by that name on this server.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = "Confirmation",
                Description = "Are you sure that you want to remove this emoji?\nReact in less than 20 seconds to confirm.",
                ThumbnailUrl = toRemove.Url
            };

            embed.AddField("Name", toRemove.Name);

            SocketReaction reaction;

            try
            {
                var sent = await ReplyAsync(embed: embed.Build());

                ChoiceTracker.AddUser(Context.User.Id);

                await sent.AddReactionAsync(reactionYes);
                await sent.AddReactionAsync(reactionNo);

                reaction = await ReactionCollector.GrabReaction(Context.User, sent,
                    x => x.Emote.Equals(reactionYes) || x.Emote.Equals(reactionNo), 20);
            }
            finally
            {
                ChoiceTracker.RemoveUser(Context.User.Id);
            }

            if (reaction == null)
            {
                await ReplyAsync("No reaction was given, aborting.");
                return;
            }

            if (reaction.Emote.Equals(reactionYes))
            {
                try
                {
                    await Context.Guild.DeleteEmoteAsync(toRemove);
                }
                catch (HttpException)
                {
                    await ReplyAsync("I failed to remove the mentioned emoji, maybe somebody else removed it first?");
                    return;
                }

                await ReplyAsync("Emoji successfully removed.");
            }
            else
            {
                await ReplyAsync("Alright, I won't remove that emoji.");
            }
        }

        [Command("viewemoji")]
        [Summary("View an emoji from DiscordEmoji.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Cooldown(3)]
        public async Task ViewEmoji(string name)
        {
            var emoji = DiscordEmoji.EmoteFromName(name);

            if (emoji == null)
            {
                await ReplyAsync("No emoji by that name was found on DiscordEmoji." +
                    "\nPlease select a valid emoji from the catalog at https://discordemoji.com" +
                    "\n\n(The emoji name is case sensitive. Don't include the colons in your command!)");

                return;
            }

            if (!(Context.Channel is IDMChannel) && (Context.Channel as ITextChannel).IsNsfw && DiscordEmoji.GetCategoryName(emoji.Category) == "NSFW")
            {
                await ReplyAsync("Woah, that's an NSFW emoji. Use this command in an NSFW channel.");
                return;
            }

            string source;

            if (String.IsNullOrWhiteSpace(emoji.Source))
                source = "**None**";
            else if (emoji.Source.StartsWith("http://") || emoji.Source.StartsWith("https://"))
                source = $"[{emoji.Source}]({emoji.Source})";
            else
                source = $"**{emoji.Source}**";

            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = $":{emoji.Title}:",
                Url = $"https://discordemoji.com/emoji/{emoji.Slug}",
                Description = String.Join('\n', new string[]
                {
                    $"Author: **{emoji.Author}**",
                    $"Category: **{DiscordEmoji.GetCategoryName(emoji.Category)}**",
                    $"Favorites: **{emoji.Favorites}**",
                    $"Source: {source}",
                    $"\nDescription:\n*{WebUtility.HtmlDecode(emoji.Description).Trim()}*"
                }),
                ImageUrl = emoji.Image
            }.WithFooter("https://discordemoji.com", "https://discordemoji.com/assets/img/ogicon.png").Build());
        }

        [Command("searchemoji")]
        [Summary("Search for an emoji from DiscordEmoji.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Cooldown(3)]
        public async Task SearchEmoji(string name)
        {
            var emojis = DiscordEmoji.Emoji.Where(x => x.Title.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1);

            if (!emojis.Any())
            {
                await ReplyAsync("No results were found for your query.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = $"Search results for: '{name}'"
            };

            if (emojis.Count() > 20)
                embed.WithDescription("Only the first 20 results were displayed. Please refine your search query if you were looking for something else.");

            foreach (var e in emojis.Take(20))
                embed.AddField($":{e.Title}:", $"[View](https://discordemoji.com/emoji/{e.Slug})");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("listemoji")]
        [Summary("List emojis from DiscordEmoji.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Cooldown(3)]
        public async Task ListEmoji(string category, int page = 1)
        {
            if (page < 1)
            {
                await ReplyAsync("Invalid page.");
                return;
            }

            int categoryId;

            if (int.TryParse(category, out int res))
            {
                if (!DiscordEmoji.Categories.ContainsKey(res))
                {
                    await ReplyAsync($"Invalid catogory ID. You can find all categories with the categories command.\n``{Configuration.Prefix}categories``");
                    return;
                }
                categoryId = res;
            }
            else
            {
                if (!DiscordEmoji.Categories.Any(x => String.Equals(category, x.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    await ReplyAsync($"Invalid category. Try using the category number. You can find all categories with the categories command.\n``{Configuration.Prefix}categories``");
                    return;
                }
                categoryId = DiscordEmoji.Categories.First(x => String.Equals(category, x.Value, StringComparison.OrdinalIgnoreCase)).Key;
            }

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = $"Category: {DiscordEmoji.GetCategoryName(categoryId)} ({categoryId})"
            };


            var actualPage = page - 1;

            var emojis = DiscordEmoji.Emoji.Where(x => x.Category == categoryId);

            var totalPages = (emojis.Count() / 10) + 1;

            var taken = emojis.Skip(actualPage * 10).Take(10);

            if (!taken.Any())
            {
                embed.Description = "Nothing was found on this page.";
            }
            else
            {
                foreach (var e in taken)
                    embed.AddField($":{e.Title}:", $"[View](https://discordemoji.com/emoji/{e.Slug})");
            }

            embed.WithFooter($"Page: {page} | {(page >= totalPages ? "(Last Page)" : $"View the next page with {Configuration.Prefix}emojis <category> {page + 1}")}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("categories")]
        [Summary("Display all DiscordEmoji emoji categories.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Cooldown(3)]
        public async Task Categories() =>
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Emoji Categories",
                Description = String.Join('\n', DiscordEmoji.Categories.OrderBy(x => x.Key).Select(x => $"ID: {x.Key} | {x.Value}"))
            }.Build());

        [Command("discordemoji")]
        [Summary("Displays DiscordEmoji statistics.")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Cooldown(3)]
        public async Task _DiscordEmoji()
        {
            var s = DiscordEmoji.Statistics;
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "DiscordEmoji Statistics"
            }
            .AddField("Emoji Count", s.Emoji.ToString())
            .AddField("Favorites Count", s.Favorites.ToString())
            .AddField("User Count", s.Users.ToString())
            .AddField("Pending Approvals", s.PendingApprovals.ToString())
            .WithFooter("https://discordemoji.com", "https://discordemoji.com/assets/img/ogicon.png")
            .Build());
        }
    }
}
