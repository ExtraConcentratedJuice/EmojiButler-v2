using Newtonsoft.Json;

namespace EmojiButlerRewrite.Entities
{
    public class DiscordEmojiStatistics
    {
        [JsonProperty("emoji")]
        public int Emoji { get; }

        [JsonProperty("users")]
        public int Users { get; }

        [JsonProperty("faves")]
        public int Favorites { get; }

        [JsonProperty("pending_approvals")]
        public int PendingApprovals { get; }

        public DiscordEmojiStatistics(int emoji, int users, int favorites, int pendingApprovals)
        {
            Emoji = emoji;
            Users = users;
            Favorites = favorites;
            PendingApprovals = pendingApprovals;
        }
    }
}
