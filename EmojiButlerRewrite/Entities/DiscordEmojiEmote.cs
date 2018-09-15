using Newtonsoft.Json;

namespace EmojiButlerRewrite.Entities
{
    public class DiscordEmojiEmote
    {
        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("slug")]
        public string Slug { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("category")]
        public int Category { get; }

        [JsonProperty("faves")]
        public int Favorites { get; }

        [JsonProperty("submitted_by")]
        public string Author { get; }

        [JsonProperty("source")]
        public string Source { get; }

        [JsonProperty("image")]
        public string Image { get; }

        public bool Animated { get => Image.EndsWith(".gif"); }

        // ctor for json.net to deserialize readonly properties
        public DiscordEmojiEmote(int id, string title, string slug, string description, int category, int favorites, string author, string source, string image)
        {
            Id = id;
            Title = title;
            Slug = slug;
            Description = description;
            Category = category;
            Favorites = favorites;
            Author = author;
            Source = source;
            Image = image;
        }
    }
}
