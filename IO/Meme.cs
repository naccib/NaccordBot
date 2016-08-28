using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using DiscordBot.Priting;
using DiscordBot.Bot;
using Discord;

namespace DiscordBot.IO
{
    static class Meme
    {
        // IO stuff
        public const string FOLDER_NAME = "memes";
        private static TextChannel MemeChannel = new TextChannel("meme", ConsoleColor.Green);
        private static string QuotesFilePath { get { return Path.Combine(FOLDER_NAME, "quotes.txt"); } }
        private static List<string> Quotes;

        // init stuff
        private static bool Initialized = false;

        // meeeeeeeeemes stuff



        public static void Initialize()
        {
            if (Initialized) return;

            MemeChannel.Write("Inicializando...");

            if(!Directory.Exists(FOLDER_NAME))
            {
                Directory.CreateDirectory(FOLDER_NAME);
                MemeChannel.Write("Criei a pasta {0}.", FOLDER_NAME);
            }

            Quotes = Properties.Settings.Default.Quotes.Split('\n').ToList();

            Initialized = true;
        }

        /// <summary>
        /// Sends a random meme to the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        public static void SendRandomMemeAsync(Channel channel)
        {
            MemeChannel.Write("Mandando um meme...");

            if(channel == null)
            {
                MemeChannel.Write("Tentei manda um meme para um canal nulo.");
                return;
            }

            Initialize();

            string meme = RandomImage(FOLDER_NAME);

            if(meme == null)
            {
                MemeChannel.Write("Não achei nenhuma imagem em /" + FOLDER_NAME);
                return;
            }

            Task sendTask = new Task(async () =>
            {
                MemeChannel.Write("Mandando {0} para {1}.", meme, channel.Name);
                try
                {
                    await channel.SendFile(meme);
                }
                catch(Exception ex)
                {
                    MemeChannel.Write("Erro: {0} : {1}.", ex.Message, ex.Source);
                }
            });


            sendTask.Start();
        }

        /// <summary>
        /// Gets all images from the directory.
        /// </summary>
        /// <returns>All images.</returns>
        private static string RandomImage(string path)
        {
            string file = null;

            if (!string.IsNullOrEmpty(path))
            {
                var extensions = new string[] { ".png", ".jpg", ".gif", ".jpeg" };

                try
                {
                    var di = new DirectoryInfo(path);

                    var rgFiles = di.GetFiles("*.*")
                        .Where(f => extensions.Contains(f.Extension.ToLower()));

                    Random R = new Random();

                    MemeChannel.Write("Achei {0} imagens.", rgFiles.Count());
                    file = rgFiles.ElementAt(R.Next(0, rgFiles.Count())).FullName;
                }

                catch(Exception ex) { MemeChannel.Write("Deu ruim! {0}", ex.Message); }
            }

            return file;
        }

        /// <summary>
        /// Adds a image to the image directory.
        /// </summary>
        /// <param name="e">The object that contains all the info.</param>
        public static void AddMemeAsync(string url, User whoAdded)
        {
            if(url == null || whoAdded == null)
            {
                MemeChannel.Write("URL nulo.");
                return;
            }

            if(!url.EndsWith(".png") && !url.EndsWith(".jpg") && !url.EndsWith(".gif"))
            {
                MemeChannel.Write("{0} não termina com .png, .jpg ou .gif!", url);
                Wrapper.SendMessageAsync(whoAdded, "Seu URL tem que terminar com `.png`, `.jpg` ou `.gif`.");
                return;
            }

            Initialize();

            using (WebClient client = new WebClient())
            {
                try
                {
                    MemeChannel.Write("Baixando {0}.", url);
                    client.DownloadFileCompleted += (sender, args) =>
                    {
                        MemeChannel.Write("Download completo.");
                    };

                    client.DownloadFileAsync(new Uri(url), GetRandomName());
                }
                catch(Exception ex)
                {
                    MemeChannel.Write("Erro: {0}.", ex.Message);
                    Wrapper.SendMessageAsync(whoAdded, "Não consegui adicionar sua imagem! :(");
                }
            }
        }

        /// <summary>
        /// Pega um nome aleatório para um arquivo.
        /// </summary>
        /// <returns></returns>
        private static string GetRandomName()
        {
            return Path.Combine(FOLDER_NAME, Guid.NewGuid().ToString() + ".jpg");
        }

        public static string GetDump()
        {
            Initialize();

            MemeChannel.Write("Dumping Meme...");
            StringBuilder sb = new StringBuilder();
            Initialize();

            sb.AppendLine("==== FILES IN /memes ====");

            foreach (var file in new DirectoryInfo(FOLDER_NAME).GetFiles().OrderBy(x => x.Length))
            {
                sb.AppendLine(String.Format("  --> {0} [{1}]", file.Name, file.Length));
            }

            sb.AppendLine("==== OFENSAS/QUOTES ====");
            foreach(var quote in Quotes)
            {
                sb.AppendFormat("  --> {0}\n", quote);
            }

            return sb.ToString();
        }

        public static void AddQuote(string quote, MessageEventArgs e)
        {
            Initialize();

            if(quote == null || e == null)
            {
                MemeChannel.Write("Quote é nulo!");
                return;
            }

            if(!quote.Contains("{0}"))
            {
                Wrapper.SendMessageAsync(e.User, "Você deve usar `/addofensa (quote)`, e `(quote)` tem que conter `{0}`, que vai ser substituido pelo nome do usuário.");
                return;
            }

            MemeChannel.Write("Adicionando quote de {0}.", e.User.Name);

            Quotes.Add(quote);
            UpdateFileAsync();
        }

        public static void Ofender(User ofendido, MessageEventArgs e)
        {
            Initialize();

            if(Quotes.Count == 0)
            {
                MemeChannel.Write("Não há quotes!");
                Wrapper.SendMessageAsync(e.Channel, "Não consegui achar nenhum quote para ofender " + ofendido.Mention +  "...\nAdicione um quote com `!addquote (quote)`");
                return;
            }

            if(ofendido == null || e == null)
            {
                MemeChannel.Write("Ofendido ou argumentos é nulo.");
                return;
            }

            string ofensa = Quotes[new Random().Next(0, Quotes.Count)];

            if (ofensa.Contains("{1}"))
                ofensa = ofensa.Replace("{0}", ofendido.Mention).Replace("{1}", e.User.Mention);
            else
                ofensa = ofensa.Replace("{0}", ofendido.Mention);

            Wrapper.SendMessageAsync(e.Channel, ofensa);
        }

        public static void UpdateFileAsync()
        {
            Properties.Settings.Default.Quotes = String.Join("\n", Quotes);
            Properties.Settings.Default.Save();
        }
    }
}
