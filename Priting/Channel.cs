using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Priting
{
    class  TextChannel
    {
        public string Name { get; set; }
        public ConsoleColor PromptColor { get; set; }
        public ConsoleColor ContentColor { get; set; }

        private const ConsoleColor DEFAULT_PROMPT_COLOR = ConsoleColor.White;
        private const ConsoleColor DEFAULT_CONTENT_COLOR = ConsoleColor.White;

        public TextChannel(string name)
        {
            Name = name;
            PromptColor = DEFAULT_PROMPT_COLOR;
            ContentColor = DEFAULT_CONTENT_COLOR;

            ChannelContainer.Add(this);
        }

        public TextChannel(string name, ConsoleColor promptColor)
        {
            Name = name;
            PromptColor = promptColor;
            ContentColor = DEFAULT_CONTENT_COLOR;

            ChannelContainer.Add(this);
        }

        public TextChannel(string name, ConsoleColor promptColor, ConsoleColor contentColor)
        {
            Name = name;
            PromptColor = promptColor;
            ContentColor = contentColor;

            ChannelContainer.Add(this);
        }

        public void Write(string content)
        {
            Writer.Write(content, this);
        }

        public void Write(string content, params object[] args)
        {
            Writer.Write(content, this, args);
        }
    }
}
