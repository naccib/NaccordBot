using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Net;

using DiscordBot.Priting;
using DiscordBot.Bot;

namespace DiscordBot.Utilities.Processing
{
    public static class Emojify
    {
        private static TextChannel EmojiChannel = new TextChannel("emojifyp", ConsoleColor.Blue); // redefine it here

        public static void Request(Discord.MessageEventArgs e, string imgUrl)
        {
            EmojiChannel.Write("Request!");

            if (e == null || e?.Channel == null)
            {
                EmojiChannel.Write("Argumento/canal nulo!");
                return;
            }

            if(imgUrl == null)
            {
                Wrapper.SendMessageAsync(e.Channel, String.Format("{0}, você não tem uma foto de perfil e/ou o url é inválido! :(", e.User.Mention));
                return;
            }
            
            if(EmojifyEngine.IsProcessing)
            {
                EmojiChannel.Write("Ja há um processamento em andamente, retornando.");
                Wrapper.SendMessageAsync(e.Channel, "Ja há um processamento em andamento, tente novamente mais tarde!");
                return;
            }

            if(!(imgUrl.EndsWith(".jpg") || imgUrl.EndsWith(".jpeg") || imgUrl.EndsWith(".png") || imgUrl.EndsWith(".gif")))
            {
                EmojiChannel.Write("{0} não termina com [.jpg, jpeg, .png, .gif].", imgUrl);
                Wrapper.SendMessageAsync(e.Channel, "O link **deve** terminar com um '.jpg', '.jpeg', '.png' ou '.gif'!");
                return;
            }

            using (WebClient wc = new WebClient())
            {
                wc.DownloadDataCompleted += (sender, _e) =>
                {
                    EmojiChannel.Write("Baixei {0} bytes.", _e.Result.Length);

                    Task t = new Task(async () => 
                    {
                        Bitmap bmp;

                        using (var ms = new MemoryStream(_e.Result))
                        {
                            try
                            {
                                bmp = new Bitmap(ms);

                                bmp = EmojifyEngine.ToEmoji(bmp);
                                byte[] imageData = (byte[])new ImageConverter().ConvertTo(bmp, typeof(byte[]));

                                using (var sendMemoryStream = new MemoryStream(imageData))
                                {
                                    await e.Channel.SendFile("arte.png", sendMemoryStream);
                                }
                            }
                            catch(Exception ex)
                            {
                                string exceptionString = String.Format("Exception: **{0}**\nMessage: {1}\nSource: {2}\nTrace: {3}\nHelp: {4}", ex.GetType().Name, ex.Message, ex.Source, ex.StackTrace, ex.HelpLink);


                                await e.Channel.SendMessage("Erro processando a imagem.\n\nExceção: ```c\n" + ex.Message + "```");
                            }
                        }
                    });

                    t.Start();
                };

                EmojiChannel.Write("Baixando imagem de {0}.", imgUrl);
                wc.DownloadDataAsync(new Uri(imgUrl));
            }
        } 
    }

    static class EmojifyEngine
    {
        private static TextChannel EmojiChannel = new TextChannel("emojify", ConsoleColor.Blue);

        // IO stuff
        private const string EMOJI_FOLDER = @"emojis_small\topados";
        private const int EMOJI_COUNT = 844;

        // processing stuff
        private const int CHUNK_SIZE = 64; // 64 by 64 pixels
        public static bool IsProcessing = false;

        // Data stuff
        private static Color[] Emojis = new Color[EMOJI_COUNT];

        private static bool Initialized = false;

        public static void Initialize()
        {
            if (Initialized) return;

            EmojiChannel.Write("Iniciando...");
            ProcessEmojis();

            Initialized = true;
        }

        public static Bitmap ToEmoji(Bitmap bmp)
        {
            if (bmp == null)
                return null;

            if (!Initialized)
                Initialize();

            if(IsProcessing)
            {
                throw new Exception("Ja há um processamento em andamento.");
            }

            IsProcessing = true;

            bmp = FixImage(bmp);

            int chunkDimensions = (int)Math.Sqrt(CHUNK_SIZE);

            int chunkCount = (bmp.Size.Height * bmp.Size.Width) / CHUNK_SIZE;

            Color[] chunks = new Color[chunkCount];
            Rectangle[] chunkLocations = new Rectangle[chunkCount];

            EmojiChannel.Write("Imagem carregada de tamanho: ({0}, {1})", bmp.Width, bmp.Height);
            EmojiChannel.Write("Cortando imagem em {0} pedaços de {1}.", chunkCount, CHUNK_SIZE);
            EmojiChannel.Write("Cada pedaço terá tamanho de {0} por {1} pixels.", chunkDimensions, chunkDimensions);

            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

            int counter = 0;

            for (int x = 0; x < bmp.Size.Width; x += chunkDimensions)
            {
                for (int y = 0; y < bmp.Size.Height; y += chunkDimensions)
                {
                    Rectangle chunckRect = new Rectangle(x, y, chunkDimensions, chunkDimensions);

                    Bitmap temp = bmp.Clone(chunckRect,
                        format);

                    string emoji = GetEmoji(temp.AverageColor());

                    //Console.WriteLine("AVG dele: {0}", temp.AverageColor().ToString());
                    //Console.WriteLine("Emoji dele: {0}", emoji);

                    chunks[counter] = temp.AverageColor();
                    chunkLocations[counter] = chunckRect;

                    counter++;
                }
            }

            EmojiChannel.Write("Processei {0} chunks.", chunkCount);

            Bitmap[] emojis = GetEmojis(chunks);
            Bitmap resultImage = MountImage(emojis, chunkLocations, bmp.Size, bmp);

            IsProcessing = false;
            return resultImage;
        }

        private static void ProcessEmojis()
        {
            int counter = 0;

            foreach (string file in Directory.GetFiles(EMOJI_FOLDER))
            {
                Bitmap bmp = new Bitmap(file); // load file to bitmap
                Emojis[counter] = bmp.AverageColor();

                counter++;
            }

            //Emojis[0] = Color.FromArgb(0, 0, 0, 0);

            EmojiChannel.Write("Processed {0} emojis.", counter);
        }

        private static string GetEmoji(Color color)
        {
            ColorDifference.Method method = ColorDifference.Method.CIE76;

            int argb = color.ToArgb();

            double delta = ColorDifference.Calculate(method, argb, Emojis[0].ToArgb());
            int match = 41;

            for (int i = 1; i < Emojis.Length; ++i)
            {
                double n_delta = Math.Abs(ColorDifference.Calculate(method, argb, Emojis[i].ToArgb()));

                if (n_delta < delta)
                {
                    match = i;
                    delta = n_delta;
                }
            }

            return Path.Combine(EMOJI_FOLDER, String.Format("{0}.png", match));
        }

        private static Bitmap[] GetEmojis(Color[] colors)
        {
            Bitmap[] result = new Bitmap[colors.Length];

            for (int i = 0; i < colors.Length; ++i)
            {
                string emoji = GetEmoji(colors[i]);
                result[i] = (Bitmap)Image.FromFile(emoji);
            }

            return result;
        }

        private static Bitmap MountImage(Bitmap[] chunks, Rectangle[] locations, Size size, Bitmap original)
        {
            if (chunks.Length != locations.Length)
            {
                EmojiChannel.Write("O tamanho dos chunks não igual ao tamanho das localizações!");
                return null;
            }

            EmojiChannel.Write("Criando imagem com tamanho ({0}, {1}).", size.Width, size.Height);

            Bitmap result = new Bitmap(size.Width, size.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                for (int i = 0; i < chunks.Length; ++i)
                {
                    g.DrawImage(chunks[i], locations[i]);
                }
            }

            // do final processing

            return result;
        }

        private static Bitmap FixImage(Bitmap image)
        {
            if (image == null)
                throw new NullReferenceException("Imagem é nula!");

            //int dimensions = (int)Math.Sqrt(GetClosestSize(image.Size.Height * image.Size.Width));
            int width = GetClosestSize(image.Width);
            int heigth = GetClosestSize(image.Height);

            EmojiChannel.Write("Dimensions: ({0}, {1})", width, heigth);

            Bitmap fixedImage = image.resizeImage(heigth, width);

            return fixedImage;
        }

        private static int GetClosestSize(int n)
        {
            int factor = 64;
            int nearestMultiple =
                    (int)Math.Round(
                         (n / (double)factor),
                         MidpointRounding.AwayFromZero
                     ) * factor;

            return nearestMultiple;
        }
    }
}
