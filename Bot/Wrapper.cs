using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using DiscordBot.Properties;
using Discord;

namespace DiscordBot.Bot
{
    static class Wrapper
    {
        // writing stuff

        public static TextChannel BotChannel = new TextChannel("bot", ConsoleColor.Cyan, ConsoleColor.White);
        public static TextChannel DiscordChannel = new TextChannel("discord", ConsoleColor.Magenta, ConsoleColor.White);

        // server stuff
        public static string Token
        {
            get
            {
                return Settings.Default.Token ?? "0";
            }
            set
            {
                Settings.Default.Token = value;
                Settings.Default.Save();
            }
        }
        private static DiscordClient Client;

        // command stuff
        private static CommandParser CParser = new CommandParser('/');

        // user stuff
        private static List<ulong> IgnoredUsersIDs = new List<ulong>();

        // cah stuff
        public static bool IsGameRunning = false;

        public static void Initialize()
        {
            // load token from file here.

            Client = new DiscordClient();
            IgnoredUsersIDs = IO.SettingsLoader.IgnoredUsers.ToList();

            Client.ExecuteAndWait(async () =>
            {
                DiscordChannel.Write("Connectando com a token {0}", Token);
                
                try
                {
                    await Client.Connect(Token);
                    DiscordChannel.Write("Conectado com êxito!");

                    Client.MessageReceived += GotMessage;

                    // greet all servers!
                    //Task greatTask = new Task(GreetAllServers);
                   // greatTask.Start();
                }
                catch(Exception e)
                {
                    if(e.Message.Contains("400")) // bad request error.
                    {
                        DiscordChannel.Write("Token inválida!");
                        DiscordChannel.Write("Mude a token no arquivo {0}.", DiscordBot.IO.SettingsLoader.SETTINGS_FILE_PATH);
                    }
                    else
                    {
                        DiscordChannel.Write("Erro ao conectar: {0}", e.Message);
                        DiscordChannel.Write("Tentando denovo...");
                        Initialize();
                    }
                }
            });
        }

        private static async void GreetAllServers()
        {
            BotChannel.Write("Encontrei {0} servidores.", Client.Servers.Count());

            foreach(Server server in Client.Servers)
            {
                var geral = server.TextChannels.First(x => x.Name == "geral" || x.Name == "general");
                
                if(geral != null)
                {
                    await geral.SendMessage("Opa! Eu tô online agora. Digite /help para ajuda. ```NÃO DIGITA AGORA, NÃO TÁ IMPLEMENTADO KSKSKSKS```");
                }
            }
        }

        private static void GotMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.IsAuthor)
                return;

            Message msg = e.Message;

            BotChannel.Write("Recebi uma mensagem: {0}, {1}, {2}", msg.Text, e.Server, e.User);

            CParser.Parse(msg.Text, Client, e);
            
        }

        public static void SendMessageAsync(Channel channel, string message)
        {
            Task t = new Task(async () => await channel.SendMessage(message == "" ? "Erro: Mensagem é branca." : message));
            t.Start();
        }

        public static void SendMessageAsync(User e, string message)
        {
            Task t = new Task(async () =>
            {
                await e.SendMessage(message);
            });

            t.Start();
        }

        public static void IgnoreUser(ulong uid)
        {
            IgnoredUsersIDs.Add(uid);
        }

        public static string GetMention(string name, Channel channel)
        {
            return channel?.FindUsers(name, true).FirstOrDefault().Mention ?? name;
        }

        public static string GetMention(string name, Server server)
        {
            return server?.FindUsers(name, true).FirstOrDefault().Mention ?? name;
        }

        public static void UpdateSettings()
        {
            IO.SettingsLoader._Settings.Ignored = IgnoredUsersIDs.ToArray();
            IO.SettingsLoader._Settings.Token = Token;
        }
    }
}
