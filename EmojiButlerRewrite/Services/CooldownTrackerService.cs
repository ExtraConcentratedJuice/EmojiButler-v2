using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EmojiButlerRewrite.Services
{
    public class CooldownTrackerService
    {
        // I am the biggest brain
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>> cooldowns = 
            new ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>>();

        private readonly ConcurrentDictionary<ulong, DateTime> responseCooldowns;

        public void AddCooldown(string command, ulong user)
        {
            if (!cooldowns.ContainsKey(command))
                cooldowns[command] = new ConcurrentDictionary<ulong, DateTime>();

            cooldowns[command][user] = DateTime.Now;
        }

        public bool TryGetSecondsElapsed(string command, ulong user, out double elapsedSeconds)
        {
            elapsedSeconds = default;

            if (cooldowns.ContainsKey(command) && cooldowns[command].ContainsKey(user))
            {
                var time = cooldowns[command][user];
                elapsedSeconds = (DateTime.Now - time).TotalSeconds;
                return true;
            }

            return false;
        }
    }
}
