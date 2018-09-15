using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EmojiButlerRewrite.Services;
using Discord.WebSocket;
using System.Linq;
using EmojiButlerRewrite.Entities;
using EmojiButlerRewrite.Preconditions;

namespace EmojiButlerRewrite.Modules
{
    public class GeneralModule : ModuleBase
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; set; }
        public EmojiButlerConfiguration Configuration { get; set; }

        [Command("reportissue")]
        [Summary("Report an issue to the bot developer.")]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [Cooldown(15)]
        public async Task ReportIssue([Remainder] string issue)
        {
            var embed = new EmbedBuilder
            {
                Title = Context.Guild != null ? $"{Context.Guild.Name}" : "Direct Message",
                Description = "From Channel: " + Context.Channel.Id + "\n" + issue
            }.WithAuthor(Context.User.Username, null, Context.User.GetAvatarUrl());

            await (Client.GetChannel(415685517271891980) as ITextChannel).SendMessageAsync(embed: embed.Build());
        }

        [Command("help")]
        [Summary("Displays the bot's help page.")]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        [Cooldown(5)]
        public async Task Help()
        {
            string desc = $"The official EmojiButler manual. EmojiButler is a bot that grabs emoji for you from [DiscordEmoji](https://discordemoji.com). All commands involving the management of emojis require the user and bot to have the 'Manage Emojis' permission.\n\nTo get help on a particular command, do ``{Configuration.Prefix}help <commandName>``.";

            if (!String.IsNullOrWhiteSpace(Configuration.DblAuth))
                desc += $"\n\nIf you like my bot, vote for it on [DBL](https://discordbots.org/bot/{Configuration.BotId})!";

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = "EmojiButler Manual",
                Description = desc
            }.AddField("\u200B", "**Commands**");

            foreach (var cmd in Commands.Commands)
            {
                embed.AddField(Configuration.Prefix + cmd.Name, $"Description: {cmd.Summary ?? "None"}\nUsage: ``{Configuration.Prefix}{cmd.Name} {String.Join(' ', cmd.Parameters.Select(x => x.IsOptional ? $"[{x.Name}]" : $"<{x.Name}>"))}``");
            }

            embed.AddField("\u200B", "**Other Stuff**\nThis bot is primarily an interface to add emojis to your server from [DiscordEmoji](https://discordemoji.com), you should check it out before using the bot." +
                "\n\n*The bot's logo is a modified version of the Jenkins (https://jenkins.io/) logo, and I am required by the license to link back to it.*");

            await Context.User.SendMessageAsync(embed: embed.Build());

            if (!(Context.Channel is IDMChannel))
                await Context.Message.AddReactionAsync(new Emoji("👌"));
        }

        [Command("hi"), Summary("If I'm alive, I'll wave. :wave:")]
        [Cooldown(3)]
        public async Task Hi() => await ReplyAsync(":wave:");

        [Command("source"), Summary("Gives you my sauce :spaghetti:")]
        [Cooldown(3)]
        public async Task Source() => await ReplyAsync("https://github.com/ExtraConcentratedJuice/EmojiButler");

        [Command("info"), Summary("Gives you some information about myself. :page_facing_up:")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        [Cooldown(3)]
        public async Task Info() =>
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Information",
                Description = "Some information about EmojiButler.",
                ThumbnailUrl = Client.CurrentUser.GetAvatarUrl()
            }
            .AddField("Library", "discord.net v2.0.0")
            .AddField("Creator", "ExtraConcentratedJuice")
            .AddField("Server Count", Client.Guilds.Count())
            .Build());

        [Command("server"), Summary("Displays an invite to the EmojiButler server.")]
        [Cooldown(3)]
        public async Task Server() => await ReplyAsync("https://discord.gg/Ushqydb");

        [Command("invite"),Summary("Displays a link to invite me to your server.")]
        [Cooldown(3)]
        public async Task Invite() => await ReplyAsync("https://discordapp.com/oauth2/authorize?client_id=415637632660537355&scope=bot&permissions=1073794112");
    }
}
