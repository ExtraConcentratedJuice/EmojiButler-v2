using Newtonsoft.Json;

namespace EmojiButlerRewrite.Entities
{
    public class DiscordEmojiEmote
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category")]
        public int Category { get; set; }

        [JsonProperty("faves")]
        public int Favorites { get; set; }

        [JsonProperty("submitted_by")]
        public string Author { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        public bool Animated { get => Image.EndsWith(".gif"); }

        public DiscordEmojiEmote()
        { }
    }
}
