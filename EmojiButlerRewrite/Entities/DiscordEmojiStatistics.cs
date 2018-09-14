using Newtonsoft.Json;

namespace EmojiButlerRewrite.Entities
{
    public class DiscordEmojiStatistics
    {
        [JsonProperty("emoji")]
        public int Emoji { get; set; }

        [JsonProperty("users")]
        public int Users { get; set; }

        [JsonProperty("faves")]
        public int Favorites { get; set; }

        [JsonProperty("pending_approvals")]
        public int PendingApprovals { get; set; }

        public DiscordEmojiStatistics()
        { }
    }
}
