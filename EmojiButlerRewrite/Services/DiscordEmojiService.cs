using EmojiButlerRewrite.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EmojiButlerRewrite.Services
{
    public class DiscordEmojiService
    {
        private readonly HttpClient client;
        private CancellationTokenSource cancellation;

        private List<DiscordEmojiEmote> emoji;
        private Dictionary<int, string> categories;

        public ReadOnlyCollection<DiscordEmojiEmote> Emoji { get => new ReadOnlyCollection<DiscordEmojiEmote>(emoji); }
        public ReadOnlyDictionary<int, string> Categories { get => new ReadOnlyDictionary<int, string>(categories); }
        public DiscordEmojiStatistics Statistics { get; private set; }

        public DiscordEmojiService()
        {
            this.client = new HttpClient
            {
                BaseAddress = new Uri("https://discordemoji.com/api")
            };
        }

        private async Task<List<DiscordEmojiEmote>> GetEmojisAsync() => await HttpGetAsync<List<DiscordEmojiEmote>>(null).ConfigureAwait(false);
        private async Task<Dictionary<int, string>> GetCategoriesAsync() => await HttpGetAsync<Dictionary<int, string>>("categories").ConfigureAwait(false);
        private async Task<DiscordEmojiStatistics> GetStatisticsAsync() => await HttpGetAsync<DiscordEmojiStatistics>("stats").ConfigureAwait(false);

        public string GetCategoryName(int num) => Categories[num];
        public bool TryGetCategoryName(int num, out string val) => Categories.TryGetValue(num, out val);

        public async Task<Stream> GetImageAsync(DiscordEmojiEmote emote) => new MemoryStream(await client.GetByteArrayAsync(emote.Image));

        public DiscordEmojiEmote EmoteFromName(string name) => Emoji.FirstOrDefault(x => x.Title == name);

        private async Task<T> HttpGetAsync<T>(string requestType, Dictionary<string, string> parameters)
        {
            var query = new Dictionary<string, string>();

            if (requestType != null)
                query.Add("request", requestType);

            if (parameters != null)
                foreach (var kvp in parameters)
                    query.Add(kvp.Key, kvp.Value);

            var queryString = new FormUrlEncodedContent(query).ReadAsStringAsync().Result;

            var resp = await client.GetStringAsync("?" + queryString);

            return JsonConvert.DeserializeObject<T>(resp);
        }

        private async Task<T> HttpGetAsync<T>(string requestType) => await HttpGetAsync<T>(requestType, null);

        private async Task RefreshEmojiRoutine()
        {
            while (true)
            {
                try
                {
                    await RefreshEmoji();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to fetch emojis: " + e);
                }
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }
        
        private async Task RefreshEmoji()
        {
            Statistics = await GetStatisticsAsync();
            emoji = await GetEmojisAsync();
            categories = await GetCategoriesAsync();
        }

        public async Task Start()
        {
            cancellation = new CancellationTokenSource();
            var t = Task.Run(RefreshEmojiRoutine, cancellation.Token);
        }
    }
}
