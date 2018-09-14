using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmojiButlerRewrite.Services
{
    public class ChoiceTrackerService
    {
        private readonly HashSet<ulong> users = new HashSet<ulong>();

        public bool HasOpenChoice(ulong id)
        {
            lock (users)
                return users.Contains(id);
        }

        public void AddUser(ulong id)
        {
            lock (users)
                users.Add(id);
        }

        public void RemoveUser(ulong id)
        {
            lock (users)
                users.Remove(id);
        }
    }
}
