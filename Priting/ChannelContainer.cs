using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Priting
{
    static class ChannelContainer
    {
        private static List<TextChannel> Channels = new List<TextChannel>();

        public static void Add(TextChannel ch)
        {
            if (Channels.Where(x => x.Name == ch.Name).Count() > 0)
                throw new Exception("Channels already contains a channel named " + ch.Name);

            Channels.Add(ch);
        }

        public static TextChannel Get(string name)
        {
            return Channels.Where(x => x.Name == name).First() ?? new TextChannel("default");
        }
    }
}
