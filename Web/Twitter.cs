using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using DiscordBot.Properties;
using DiscordBot.Bot;
using Discord;
using Tweetinvi;
using Tweetinvi.Streaming;

namespace DiscordBot.Web
{
    static class Twitter
    {
        // IO stuff
        private static bool Initialized = false;
        private static TextChannel TwitterChannel = new TextChannel("twitter", ConsoleColor.Cyan);

        // memes stuff
        private static List<string> Accounts;

        public static void Initialize()
        {
            if (Initialized) return;

            TwitterChannel.Write("Iniciando o Twitter...");

            var credenciais = Auth.SetUserCredentials (Settings.Default.TWITTER_CONSUMER_KEY, 
                Settings.Default.TWITTER_CONSUMER_SECRET, 
                Settings.Default.TWITTER_AC, 
                Settings.Default.TWITTER_AC_SECRET);
           
            if(credenciais == null)
            {
                TwitterChannel.Write("Não consegui logar no Twitter!");
                Initialized = false;
                return;
            }
            else
            {
                TwitterChannel.Write("Logado com sucesso!");
                Initialized = true;
            }

            if (Settings.Default.TwitterAccounts == null)
                Accounts = new List<string>();
            else
                Accounts = Settings.Default.TwitterAccounts.Split(',').ToList();

            if (Accounts.Contains(""))
                Accounts.Remove("");

            if (Accounts.Contains(" "))
                Accounts.Remove(" ");
        }

        public static void SendRandomMemeAsync(Channel ch)
        {
            Initialize();

            if (Accounts.Count == 0)
            {               
                TwitterChannel.Write("Nenhuma conta encontrada.");
                Wrapper.SendMessageAsync(ch, "Adicione uma conta antes com `addaccount (conta)`!");
            }
            else
            {
                TwitterChannel.Write("Achei {0} contas:", Accounts.Count);

                foreach (string conta in Accounts)
                    TwitterChannel.Write(" -> {0}", conta);
            }

            var parameters = Search.CreateTweetSearchParameter(String.Format("from:{0} filter:media", GetRandomAccount()));
            parameters.TweetSearchType = Tweetinvi.Parameters.TweetSearchType.OriginalTweetsOnly;

            List<string> mediaUrls = new List<string>();
            var tweets = Search.SearchTweets(parameters).Where(x => x.Media.Count() != 0).ToList();

            TwitterChannel.Write("Achei {0} tweets.", tweets.Count);

            var choosenTweet = tweets[new Random().Next(0, tweets.Count)];

            TwitterChannel.Write("MEDIAS ACHADAS:");
            foreach (var media in choosenTweet.Media)
            {
                TwitterChannel.Write(" -> {0}", media.MediaURL);
                mediaUrls.Add(media.MediaURL);
            }

            DownloadAndSendImage(mediaUrls.ToArray(), choosenTweet, ch);
        }

        private static void DownloadAndSendImage(string[] urls, Tweetinvi.Models.ITweet tweet, Channel destination)
        {
            if(tweet == null || destination == null)
            {
                TwitterChannel.Write("Tweet ou canal é nulo!");
                return;
            }

            TwitterChannel.Write("Baixando o tweet de {0}.", tweet.CreatedBy.Name);

            using (WebClient client = new WebClient())
            {
                foreach(string url in urls)
                {
                    client.DownloadDataCompleted += async (sender, e) =>
                    {
                        byte[] data = e.Result;
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            TwitterChannel.Write("Mandando imagem com {0} bytes para {1}.", data.Length, destination.Name);
                            await destination.SendFile("meme.jpg", stream);
                        }
                    };

                    client.DownloadDataAsync(new Uri(url));
                }

                Wrapper.SendMessageAsync(destination, String.Format("\"{0}\", por {1}.", tweet.FullText, tweet.CreatedBy.Name));
            }
        }

        public static void AddAccount(string accountName, Discord.User sender)
        {
            Initialize();

            if(accountName == null)
            {
                TwitterChannel.Write("Nome da conta nula!");
                return;
            }

            Task addTask = new Task(async () =>
            {
                if(await UserExists(accountName) && !AccountAlreadyExists(accountName))
                {
                    TwitterChannel.Write("Adicionando conta {0}.", accountName);

                    Accounts.Add(accountName);

                    Settings.Default.TwitterAccounts = String.Join(",", Accounts);
                    Settings.Default.Save();
                }
                else
                {
                    TwitterChannel.Write("Não achei o usuário {0}.", accountName);
                    await sender.SendMessage("O usuário " + accountName + " não existe ou não tuitou nas útilmas 2 semanas! :(");
                }
            });

            addTask.Start();
        }

        private static string GetRandomAccount()
        {
            return Accounts[new Random().Next(0, Accounts.Count)];
        }

        private static async Task<bool> UserExists(string username)
        {
            var query = await SearchAsync.SearchTweets("from:@" + username);

            if (query == null)
                return false;

            if (query.Count() == 0)
                return false;
            else
                return true;
        }

        private static bool AccountAlreadyExists(string username)
        {
            if(username == null)
            {
                TwitterChannel.Write("Não se pode checar um usuário nulo.");
                return false;
            }

            return Accounts.Contains(username);
        }
    }
}
