using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using Discord;

namespace DiscordBot.Bot
{
    class Command
    {
        public string Identifier { get; private set; }
        public string[] Arguments { get; private set; }

        public Command(string identifier, string[] arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }
    }

    class CommandParser
    {
        private TextChannel CommandChannel = new TextChannel("command");
        private char Prompt;

        public CommandParser(char commandPrompt)
        {
            Prompt = commandPrompt;
        }

        public void Parse(string str, DiscordClient client, MessageEventArgs e)
        {
            if (str == null)
                throw new ArgumentNullException("'str' is null.");

            if (!str.StartsWith(Prompt.ToString()))
                return;

            str = str.Remove(0, 1); // remove first char

            int count = 0;

            string ident = "";
            List<string> args = new List<string>(); ;

            foreach (string arg in str.Split(' '))
            {
                if (count == 0) // is first part
                    ident = arg;
                else
                    args.Add(arg);

                count++;
            }

            Command outCommand = new Command(ident, args.ToArray());

            Parse(outCommand, client, e);
        }

        public void Parse(Command cmd, DiscordClient client, MessageEventArgs e)
        {
            CommandChannel.Write("Comando '{0}' de " + e.User.Name, cmd.Identifier);
            string[] args = cmd.Arguments;

            if (cmd.Identifier == "help" || cmd.Identifier == "ajuda")
            {
                SendMessageAsync(e.User, "Comandos: ");             
            }

            if(cmd.Identifier == "role")
            {
                if(args.Length > 0)
                {
                    string roleName = args[0];

                    if(roleName == "programador")
                    {
                        SetUserRoleAsync(e.User, e.Server.FindRoles("Programador", true).ElementAt(0));
                        CommandChannel.Write("Dando o cargo Programador para {0}.", e.User.Name);
                    }
                    else if(roleName == "gamer")
                    {
                        SetUserRoleAsync(e.User, e.Server.FindRoles("Gamer", true).ElementAt(0));
                        CommandChannel.Write("Dando o cargo Gamer para {0}.", e.User.Name);
                    }
                    else if (roleName == "politico")
                    {
                        SetUserRoleAsync(e.User, e.Server.FindRoles("Político", true).ElementAt(0));
                        CommandChannel.Write("Dando o cargo Político para {0}.", e.User.Name);
                    }
                    else if(roleName == "player")
                    {
                        SetUserRoleAsync(e.User, e.Server.FindRoles("Player", true).ElementAt(0));
                        CommandChannel.Write("Dando o cargo Player para {0}.", e.User.Name);
                    }              
                }
                else
                {
                    SendMessageAsync(e.Channel, "Use `/role (role)` para mudar seu role.\nRoles: ```python\n" + " faz algo para pegar todos os roles. " + "```");
                }
            }

            if(cmd.Identifier == "cah")
            {
                Games.CAH.Initialize(client, e.Server);
            }

            if(cmd.Identifier == "answer")
            {
                Games.CAH.CAHGotMessage(e.User, String.Join(" ", cmd.Arguments));
            }

            if(cmd.Identifier == "choose")
            {
                Games.CAH.Choose(String.Join(" ", cmd.Arguments), e.User);
            }

            if(cmd.Identifier == "quit")
            {
                CommandChannel.Write("Finalizando...");
                // get admin role
                Role admRole = e.Server.Roles.FirstOrDefault(x => x.Name == Properties.Settings.Default.AdminRoleName);
                
                if(admRole == null)
                {
                    CommandChannel.Write("Não achei o role admin.\nAdicione o nome dele no arquivo settings.ini.");
                    Console.ReadKey();
                }
                else
                {
                    if (!e.User.HasRole(admRole))
                        return;

                    CommandChannel.Write("Finalizando CAH...");

                    // clean stuff up
                    //Games.CAH.DeleteChannel();
                    Games.CAH.ResetRole();

                    CommandChannel.Write("Tchau");
                    //Environment.Exit(0);
                }
                
            }
        }

        private void SendMessageAsync(User e, string message)
        {
            Task t = new Task(async () =>
            {
               await e.SendMessage(message);
            });

            t.Start();
        }

        private void SendMessageAsync(Channel ch, string message)
        {
            Task t = new Task(async () =>
            {
                await ch.SendMessage(message);
            });

            t.Start();
        }

        private void SetUserRoleAsync(User e, Role role)
        {
            /*Task t = new Task(async () =>
            {
                await e.AddRoles(role);
            });

            t.Start();*/

            e.AddRoles(role);
        }
    }
}
