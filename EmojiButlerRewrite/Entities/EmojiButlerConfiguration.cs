using System;
using System.Collections.Generic;
using System.Text;

namespace EmojiButlerRewrite.Entities
{
    public class EmojiButlerConfiguration
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public ulong IssueChannel { get; set; }
        public string DblAuth { get; set; }
        public ulong BotId { get; set; }
    }
}
