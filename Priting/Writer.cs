using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Priting
{
    static class Writer
    {
        public static void Write(string content, TextChannel channel)
        {
            Console.ForegroundColor = channel.PromptColor;
            Console.Write("[{0}] ", channel.Name.ToUpper());

            Console.ForegroundColor = channel.ContentColor;
            Console.WriteLine(content);

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Write(string content, TextChannel channel, params object[] args)
        {
            Console.ForegroundColor = channel.PromptColor;
            Console.Write("[{0}] ", channel.Name.ToUpper());

            Console.ForegroundColor = channel.ContentColor;
            Console.WriteLine(content, args);

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
