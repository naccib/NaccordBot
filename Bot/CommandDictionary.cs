using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot
{
    static class CommandDictionary
    {
        private static List<CommandInfo> Commands = new List<CommandInfo>();

        public static void Add(string cmd, string summary)
        {
            if (cmd == null || summary == null)
                throw new ArgumentNullException("cmd cannot be null!");

            Commands.Add(new CommandInfo(cmd, summary));
        }

        public static string GetInfo()
        {
            StringBuilder result = new StringBuilder();

            foreach(CommandInfo cmd in Commands)
            {
                result.AppendFormat("```python\n[{0}]: {1}.```\n", cmd.Name, cmd.Summary);
            }

            return result.ToString();
        }

        public static void Initialize()
        {
            Add("/help ou /ajuda", "Mostra esse dialogo");
            Add("/role", "Mostra todos os cargos");
            Add("/role (role)", "Entra no cargo 'role'");
        }
    }

    class CommandInfo
    {
        public string Name, Summary;
        
        public CommandInfo(string name, string summary)
        {
            Name = name;
            Summary = summary;
        }
    }
}
