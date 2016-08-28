using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Discord;
using DiscordBot.Priting;
using DiscordBot.Bot;

namespace DiscordBot.Games
{
    static class CAH
    {
        // discord stuff
        private static DiscordClient Client;
        private static Server MainServer;
        private static Channel MainChannel;
        private static TextChannel CAHChannel = new TextChannel("cah", ConsoleColor.DarkCyan, ConsoleColor.White);

        private const string CHANNEL_NAME = "CAH";

        // player stuff
        private static List<User> Players;
        private static Dictionary<string, int> Points;
        private static Role PlayerRole;


        // czar stuff
        public static ulong CzarID = 0;
        public static string CzarName = "";
        private static string ChoosenUser = string.Empty;
        public static int CzarIndex = 0;

        // cards stuff
        public static List<int> Cards;
        private const int CARDS_COUNT = 75;
        private static int CurrentCard = 0;

        // round stuff
        private const double ROUND_TIMEOUT = 60.0;
        private const int MAX_ROUNDS = 10;
        private static int RoundNumber = 1;
        private static int PlayersThatAnswered = 0;
        private static Dictionary<string, string> CurrentUserAnswers = new Dictionary<string, string>();
        private static bool ChooseTime = false;

        public static void Initialize(DiscordClient client, Server serv, bool silent = false)
        {
            CAHChannel.Write("Começando no servidor {0}...", serv.Name);

            Client = client;
            MainServer = serv;

            int[] cards = new int[CARDS_COUNT];

            for (int i = 0; i < CARDS_COUNT; ++i)
                cards[i] = i + 1;

            Cards = new List<int>(cards);
            Points = new Dictionary<string, int>();

            // start channel
            CAHChannel.Write("Procurando canal {0}...", CHANNEL_NAME);
            Channel discordChannel = serv.TextChannels.FirstOrDefault(x => x.Name == CHANNEL_NAME);

            if (discordChannel == null) // não existe
            {
                CAHChannel.Write("Canal não encontrado, criando um novo...");
                CreateChannel(silent);
            }
            else
            {
                CAHChannel.Write("Canal encontrado.");
                MainChannel = discordChannel;
            }
            //StartGame();
        }

        private static bool UpdatePlayers()
        {
            Players = MainChannel.Users.Where(x => x.HasRole(PlayerRole)).ToList();
            if(RoundNumber == 1) Players.Shuffle(); // sort the players once
        
            CAHChannel.Write("Achei {0} jogadores.", Players.Count);


            if (Players.Count < 2)
            {
                Wrapper.SendMessageAsync(MainChannel, "Você precisa de no mínimo 2 jogadores para começar o Cards Against Humanity!");
                Wrapper.SendMessageAsync(MainChannel, "Para se juntar aos jogadores, digite `/role player`!");

                return false;
            }

            foreach (User user in Players)
            {
                if(!Points.ContainsKey(user.Name))
                    Points.Add(user.Name, 0);
            }

            Task editTopic = new Task(async () => await MainChannel.Edit(topic: GetTopic()));
            editTopic.Start();

            return true;
        }

        public static Role CreatePlayerRole()
        {
            if (MainServer.Roles.Where(x => x.Name == "Player").Count() > 0)
                PlayerRole = MainServer.Roles.First(x => x.Name == "Player");
            else
                PlayerRole = MainServer.CreateRole("Player", null, null, true, true).Result;

            CAHChannel.Write("Criei/achei role 'Player'.");

            return PlayerRole;
        }

        public static void ResetRole()
        {
            CAHChannel.Write("Começando a remoção de 'Player'");

            foreach (User u in UsersThatHaveRole("Player"))
            {
                CAHChannel.Write("Removendo 'Player' de {0}.", u.Name);
                u.RemoveRoles(MainServer.Roles.FirstOrDefault(x => x.Name == "Player"));
            }

            CAHChannel.Write("Terminou a remoção.");
        }

        public static List<User> UsersThatHaveRole(string roleName)
        {
            try
            {
                List<User> l = MainServer.Users.Where(x => x.HasRole(MainServer.Roles.FirstOrDefault(y => y.Name == roleName))).ToList();
                CAHChannel.Write("Found {0} usrs with role {1}.", l.Count, roleName);

                return l;
            }
            catch
            {
                return new List<User>();
            }
        }

        public static void StartGame()
        {
            if(MainServer == null || MainChannel == null || Client == null)
            {
                CAHChannel.Write("MainServer, MainChannel ou Client é nulo, impossível começar o jogo.");
                return;
            }

            RunRound();
        }

        public static void RunRound()
        {
            Wrapper.IsGameRunning = true;

            Action roundAction = async () =>
            {
                ChooseTime = false;

                if(!UpdatePlayers())
                {
                    return;
                }

                CAHChannel.Write("Started round {0}.", RoundNumber);
                string mention = ChooseCzar();

                Wrapper.SendMessageAsync(MainChannel, "O Round **" + RoundNumber.ToString() + "** começou!");
                SendCardToChannel();
                Wrapper.SendMessageAsync(MainChannel, "**Não** sabe jogar?! Leia: http://docdro.id/gfSq9Aj.");
                Wrapper.SendMessageAsync(MainChannel, "O czar atual é @" + mention + "!");

                while(PlayersThatAnswered != Players.Count - 1)
                { 
                    await Task.Delay(100);
                }

                
                CAHChannel.Write("Round acabou!");

                ChooseTime = true;


                Wrapper.SendMessageAsync(MainChannel, "Todos responderam! " + PlayerRole.Mention);

                StringBuilder message = new StringBuilder();

                foreach (KeyValuePair<string, string> kv in CurrentUserAnswers)
                    message.AppendLine(String.Format("**{0}**:```{1}```", Wrapper.GetMention(kv.Key, MainChannel), kv.Value));

                Wrapper.SendMessageAsync(MainChannel, message.ToString() + ".");
                Wrapper.SendMessageAsync(MainChannel, String.Format("{0}, qual você prefere?\nEscreva `/choose username` para escolher a melhor resposta!", mention));    
            };

            Task RoundTask = new Task(roundAction);
            RoundTask.Start();            
        }

        public static void CAHGotMessage(User user, string answer)
        {
            CAHChannel.Write("Adicionando resposta do {0}.", user.Name);
            
            if(user.Id == CzarID)
            {
                CAHChannel.Write("O czar tentou mandar um mensagem...");
                Wrapper.SendMessageAsync(user, "Você é o czar!");
                return;
            }


            // handle answer
            if (CurrentUserAnswers.ContainsKey(user.Name))
                CurrentUserAnswers[user.Name] = answer;
            else
            {
                CurrentUserAnswers.Add(user.Name, answer);
                PlayersThatAnswered++;
            }

            Wrapper.SendMessageAsync(user, "Sua resposta é " + answer + " agora!");
            
            PrintAnswers();
        }

        private static void PrintAnswers()
        {
            CAHChannel.Write("Current answers are:");

            foreach (KeyValuePair<string, string> kv in CurrentUserAnswers)
                CAHChannel.Write("--> {0} : {1}", kv.Key, kv.Value);
        }

        private static void SendCardToChannel()
        {
            CurrentCard = new Random().Next(0, CARDS_COUNT);
            string cardPath = System.IO.Path.Combine("cards", String.Format("{0}.png", CurrentCard + 1));

            Task t = new Task(async () =>
            {
                await MainChannel.SendFile(cardPath);
            });

            t.Start();
        }

        private static string ChooseCzar()
        { 
            User czar = Players[CzarIndex];

            if (CzarIndex + 1 == Players.Count)
                CzarIndex = 0;
            else
                CzarIndex++;

            CzarID = czar.Id;
            CzarName = czar.Name;

            return czar.Mention;
        }

        private static string GetTopic()
        {
            StringBuilder builder = new StringBuilder();
            List<User> players = UsersThatHaveRole("Player");

            if (players.Count < 2)
                return "You need at least 2 players to play this game.\nPlayers can be added with `/role player`.";

            builder.Append("Players: ");

            foreach(User player in players)
            {
                if (players.Last() == player)
                    builder.Append(player.Mention + ".");
                else
                    builder.Append(player.Mention + ", ");
            }

            return builder.ToString();
        }

        public static void CreateChannel(bool silent)
        {
            MainChannel = MainServer.CreateChannel(CHANNEL_NAME, ChannelType.Text).Result;

            Wrapper.SendMessageAsync(MainChannel, "Hey! Bem vindo ao **C**ards **A**gainst **H**umanity, feito pelo " + Wrapper.GetMention("naccib", MainChannel) + ".\n\nO source code está no GitHub: https://github.com/naccib/NaccordBot. \nSiga o naccib no Twitter: https://twitter.com/vlwvlwvlwvlwvlw.");

            Task changeTopic = new Task(async () => await MainChannel.Edit(topic: GetTopic()));
            changeTopic.Start();

            CreatePlayerRole();
            if(!silent) TrySendInviteAsync();     

            StartGame();
        }

        public static void TrySendInviteAsync()
        {
            Channel mainCh = MainServer.TextChannels.FirstOrDefault(x => x.Name == "geral" || x.Name == "general");

            if (mainCh == null)
                return;

            Wrapper.SendMessageAsync(mainCh, "Opa! Ta rolando um **C**ards **A**gainst **H**umanity no " + MainChannel.Mention + "!\nDigite `/role player` e vá para o canal " + MainChannel.Mention + " para jogar!");
        }

        public static void DeleteChannel()
        {
            MainChannel.Delete();
        }

        public static void Choose(string username, User sender, bool force = false)
        {
            if(sender.Id != CzarID && !force)
            {
                CAHChannel.Write("Alguem tentou escolher pelo czar.");
                Wrapper.SendMessageAsync(sender, "Você não é o czar para escolher o usuário!");
                return;
            }

            if (!ChooseTime)
            {
                CAHChannel.Write("O czar tentou escolher antes de acabar o tempo.");
                Wrapper.SendMessageAsync(MainChannel, "Espere todos responderem antes de escolher o ganhador!");
                return;
            }

            if(Players.Where(x => x.Name == username).Count() == 0)
            {
                CAHChannel.Write("O czar tentou escolher um usuário que não é jogador.");
                Wrapper.SendMessageAsync(MainChannel, "Escolhe um dos usuários que sejam jogadores!");
                return;
            }


            ChoosenUser = username;

            try
            {
                if (Points.ContainsKey(username))
                    Points[username]++;
                else
                    Points.Add(username, 1);

                Wrapper.SendMessageAsync(MainChannel, "O ganhador foi **" + ChoosenUser + "**!");
                CAHChannel.Write("Ganhador escolhido: {0}", ChoosenUser);
            }
            catch(Exception)
            {
                CAHChannel.Write("Could not find user {0} in Points list.", username);
            }

            Wrapper.SendMessageAsync(MainChannel, GetPlayersFormatted());

            ChooseTime = false;
            EndRound();
        }

        private static string GetPlayersFormatted()
        {
            StringBuilder resultado = new StringBuilder();

            resultado.AppendLine("Pontuação: ```");

            foreach(var player in Players)
            {
                if (!Points.ContainsKey(player.Name))
                    Points.Add(player.Name, 0);

                resultado.AppendFormat("{0} - {1}\n", player.Name, Points[player.Name]);
            }

            resultado.Append("```");

            if (resultado.Length > 2000)
                return "Muito grande.";

            return resultado.ToString();
        }

        public static void EndRound()
        {
            CurrentUserAnswers.Clear();
            PlayersThatAnswered = 0;
            ChoosenUser = string.Empty;

            RoundNumber++;

            if (RoundNumber <= MAX_ROUNDS)
                RunRound();
            else
            {
                Wrapper.IsGameRunning = false;
                ResetRole();
            }
        }

        public static void ForceEnd()
        {
            PlayersThatAnswered = Players.Count() - 1;
        }

        public static void ResetPlayers()
        {
            Task deleteTask = new Task(async () =>
            {
                await PlayerRole.Delete();
                CreatePlayerRole();

                Wrapper.SendMessageAsync(MainChannel, "**Limpando jogadores!**\n\nSe você deseja continuar jogando, digite `/role player`.");
            });

            deleteTask.Start();
        }

        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
