using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using DiscordBot.Web.YTS;
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
        public Games.CompleteTheCode CTC;

        // history stuff
        private List<string> CommandHistory = new List<string>();

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
                SendMessageAsync(e.User, Help.GetCommandString());             
            }

            if (cmd.Identifier == "role")
            {
                if (args.Length > 0)
                {
                    string roleName = args[0];
                    try
                    {
                        if (roleName == "programador")
                        {
                            SetUserRoleAsync(e.User, e.Server.FindRoles("Programador", true).ElementAt(0));
                            CommandChannel.Write("Dando o cargo Programador para {0}.", e.User.Name);
                        }
                        else if (roleName == "gamer")
                        {
                            SetUserRoleAsync(e.User, e.Server.FindRoles("Gamer", true).ElementAt(0));
                            CommandChannel.Write("Dando o cargo Gamer para {0}.", e.User.Name);
                        }
                        else if (roleName == "politico")
                        {
                            SetUserRoleAsync(e.User, e.Server.FindRoles("Político", true).ElementAt(0));
                            CommandChannel.Write("Dando o cargo Político para {0}.", e.User.Name);
                        }
                        else if (roleName == "player")
                        {
                            Role playerRole = e.Server.FindRoles("Player", true).FirstOrDefault();

                            if (playerRole == null)
                                playerRole = Games.CAH.CreatePlayerRole();

                            SetUserRoleAsync(e.User, playerRole);
                            CommandChannel.Write("Dando o cargo Player para {0}.", e.User.Name);
                        }
                    }
                    catch(Exception ex)
                    {
                        CommandChannel.Write("Erro: {0} : {1}.", ex.Message, ex.Source);
                    }
                }

            else
            {
                SendMessageAsync(e.Channel, "Use `/role (role)` para mudar seu role.\nRoles:```c\nprogramador\ngamer\npolitico```");
            }
            }

            if(cmd.Identifier == "roles")
            {
                SendMessageAsync(e.Channel, "Os cargos são:\n```c\nprogramador\ngamer\npolitico``` Use `/role (role)` para adicionar um a você!");
            }

            if(cmd.Identifier == "cah")
            {
                if (cmd.Arguments[0] != null && cmd.Arguments[0] == "-silent")
                    Games.CAH.Initialize(client, e.Server, true);
                else
                    Games.CAH.Initialize(client, e.Server, false);
            }

            if(cmd.Identifier == "answer")
            {
                Games.CAH.CAHGotMessage(e.User, String.Join(" ", cmd.Arguments));
            }

            if(cmd.Identifier == "choose")
            {
                Games.CAH.Choose(String.Join(" ", cmd.Arguments), e.User);
            }

            if(cmd.Identifier == "force_end")
            {
                if (!IsAdm(e))
                    return;

                CommandChannel.Write("Terminando o jogo a força.");
                Games.CAH.ForceEnd();
            }

            if(cmd.Identifier == "reset_players")
            {
                if (!IsAdm(e))
                    return;

                Games.CAH.ResetPlayers();              
            }

            if(cmd.Identifier == "force_choose")
            {
                if (!IsAdm(e))
                    return;

                Games.CAH.Choose(String.Join(" ", cmd.Arguments), e.User, true);
            }

            if(cmd.Identifier == "ctc")
            {
                if (!IsAdm(e))
                    return;

                CTC = new Games.CompleteTheCode(e);
            }

            if(cmd.Identifier == "play")
            {
                if(CTC == null)
                {
                    CommandChannel.Write("O CTC é nulo.");
                    return;
                }

                CTC.AddPlayer(e.User);
            }

            if(cmd.Identifier == "code")
            {
                if (CTC == null)
                {
                    CommandChannel.Write("O CTC é nulo.");
                    return;
                }

                CTC.AddCode(String.Join(" ", cmd.Arguments), e.User);
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

            if(cmd.Identifier == "google")
            {
                Web.GoogleSearch.Search(String.Join(" ", cmd.Arguments), e.Channel);
            }

            if(cmd.Identifier == "meme")
            {
                if(cmd.Arguments.Length == 0)
                    IO.Meme.SendRandomMemeAsync(e.Channel);
                else
                {
                    if(cmd.Arguments[0] == "twitter")
                    {
                        Web.Twitter.SendRandomMemeAsync(e.Channel);
                    }
                    else if(cmd.Arguments[0] == "local")
                    {
                        IO.Meme.SendRandomMemeAsync(e.Channel);
                    }
                }
            }

            if(cmd.Identifier == "addmeme")
            {
                IO.Meme.AddMemeAsync(String.Join(" ", cmd.Arguments), e.User);
            }

            if(cmd.Identifier == "addaccount")
            {
                Web.Twitter.AddAccount(String.Join(" ", cmd.Arguments), e.User);
            }


            if (cmd.Identifier == "accounts")
            {
                string result = "";
                var accounts = Properties.Settings.Default.TwitterAccounts.Split(',').ToList();

                result += String.Format("Achei {0} contas: \n", accounts.Count);
                result += "```c\n";

                foreach (string account in accounts)
                    result += String.Format("{0}. {1}\n", accounts.IndexOf(account) + 1, account);

                result += "```";

                Wrapper.SendMessageAsync(e.User, result);
            }

            if (cmd.Identifier == "addofensa")
            {
                IO.Meme.AddQuote(String.Join(" ", cmd.Arguments), e);
            }

            if(cmd.Identifier == "addcantada")
            {
                Cantadas.AddCantada(e, String.Join(" ", cmd.Arguments));
            }

            if(cmd.Identifier == "ofender")
            {
                ulong id = 0;
                if (UInt64.TryParse(String.Join(" ", cmd.Arguments), out id))
                {
                    User ofendido = e.Channel.GetUser(id);

                    if (ofendido == null)
                    {
                        Wrapper.SendMessageAsync(e.Channel, "Não consegui achar um usuário com nome \"" + String.Join(" ", cmd.Arguments) + "\"");
                        return;
                    }

                    IO.Meme.Ofender(ofendido, e);
                }
                else
                {
                    User ofendido = e.Channel.FindUsers(String.Join(" ", cmd.Arguments), false).FirstOrDefault();

                    if (ofendido == null)
                    {
                        Wrapper.SendMessageAsync(e.Channel, "Não consegui achar um usuário com nome \"" + String.Join(" ", cmd.Arguments) + "\"");
                        return;
                    }

                    IO.Meme.Ofender(ofendido, e);
                }
            }

            if(cmd.Identifier == "cantar")
            {
                ulong id = 0;
                if (UInt64.TryParse(String.Join(" ", cmd.Arguments), out id))
                {
                    User ofendido = e.Channel.GetUser(id);

                    if (ofendido == null)
                    {
                        Wrapper.SendMessageAsync(e.Channel, "Não consegui achar um usuário com nome \"" + String.Join(" ", cmd.Arguments) + "\"");
                        return;
                    }

                    Cantadas.Cantar(ofendido, e);
                }
                else
                {
                    User ofendido = e.Channel.FindUsers(String.Join(" ", cmd.Arguments), false).FirstOrDefault();

                    if (ofendido == null)
                    {
                        Wrapper.SendMessageAsync(e.Channel, "Não consegui achar um usuário com nome \"" + String.Join(" ", cmd.Arguments) + "\"");
                        return;
                    }

                    Cantadas.Cantar(ofendido, e);
                }
            }

            if(cmd.Identifier == "twitter")
            {
                Web.Twitter.SendRandomMemeAsync(e.Channel);
            }
            
            if(cmd.Identifier == "latex")
            {
                Web.LaTeX.ConvertEquationAndSendAsync(String.Join(" ", cmd.Arguments), e.Channel, false);
            }

            if (cmd.Identifier == "latex2")
            {
                Web.LaTeX.ConvertEquationAndSendAsync(String.Join(" ", cmd.Arguments), e.Channel, true);
            }

            if (cmd.Identifier == "dump")
            {
                IO.Dumper.SendDump(e.User, GetDump());
            }

            if(cmd.Identifier == "eval")
            {
                try
                {
                    double result = Utilities.EquationSolver.Expression(String.Join(" ", cmd.Arguments));
                    Wrapper.SendMessageAsync(e.Channel, String.Format("{0} = {1}", String.Join(" ", cmd.Arguments), result));
                }
                catch(Exception ex)
                {
                    Wrapper.SendMessageAsync(e.User, ex.Message);
                }
            }

            if(cmd.Identifier == "movies")
            {
                string query = String.Join(" ", cmd.Arguments);

                Task sendTask = new Task(async () =>
                {
                    var movies = YTSClient.GetMovies(query);
                    
                    if(movies.Length == 0)
                    {
                        await e.Channel.SendMessage("Não existe nenhum filme com essa query, tente denovo!");
                    }
                    else
                    {
                        YTSClient.SendMovies(movies, e.Channel);
                    }
                });

                sendTask.Start();
            }

            if(cmd.Identifier == "emojify")
            {
                string _args = String.Join(" ", cmd.Arguments);

                if(IsImageUrl(_args))
                    Utilities.Processing.Emojify.Request(e, _args);
                else
                {
                    ulong id = 0;
                    if (UInt64.TryParse(_args, out id))
                    {
                        User usuario = e.Channel.GetUser(id);

                        if (usuario == null)
                        {
                            Wrapper.SendMessageAsync(e.Channel, "Não consegui achar um usuário com nome \"" + String.Join(" ", cmd.Arguments) + "\"");
                            return;
                        }

                        Utilities.Processing.Emojify.Request(e, usuario.AvatarUrl);
                    }
                    else
                    {
                        User usuario = e.Channel.FindUsers(_args, false).FirstOrDefault();

                        if (usuario == null)
                        {
                            Wrapper.SendMessageAsync(e.Channel, "Não consegui achar um usuário com nome \"" + String.Join(" ", cmd.Arguments) + "\"");
                            return;
                        }

                        Utilities.Processing.Emojify.Request(e, usuario.AvatarUrl);
                    }
                }

            }

            CommandHistory.Add(String.Format("[{0}] {1} por {2} em {3} -> {4} [{5}].", 
                DateTime.Now.ToLongTimeString(),
                String.Format("{0} {1}", cmd.Identifier, String.Join(" ", cmd.Arguments)),
                e.User.Name == null ? "No User" : e.User.Name,
                e.Channel == null ? "No Channel" : e.Channel.Name,
                e.Server == null ? "No Server" : e.Server.Name,
                e.Server == null ? "No ID" : e.Server.Id.ToString()
                ));

            Wrapper.CommandMessages.Add(e.Message);
        }

        public string GetDump()
        {
            StringBuilder dump = new StringBuilder();

            dump.AppendLine("==== HISTÓRICO DE COMANDOS ====");
            dump.AppendLine(String.Format("Achei {0} comandos: ", CommandHistory.Count));

            foreach(var command in CommandHistory)
            {
                dump.AppendLine(String.Format("  --> {0}", command));
            }

            return dump.ToString();
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

        private bool IsAdm(MessageEventArgs e)
        {
            Role admRole = e.Server.Roles.FirstOrDefault(x => x.Name == Properties.Settings.Default.AdminRoleName);

            if (admRole == null)
            {
                CommandChannel.Write("Não achei o role admin.\nAdicione o nome dele no arquivo settings.ini.");
                return false;
            }
            else
            {
                if (!e.User.HasRole(admRole))   
                    return false;
            }

            return true;
        }

        private bool IsImageUrl(string str)
        {
            return str.EndsWith(".jpg") || str.EndsWith(".jpeg") || str.EndsWith(".png") || str.EndsWith(".gif");
        }
    }
}
