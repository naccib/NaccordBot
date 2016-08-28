using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Drawing;

using DiscordBot.Priting;
using DiscordBot.Bot;
using Discord;

namespace DiscordBot.Web
{
    static class LaTeX
    {
        private const string URL_FORMAT = "https://latex.codecogs.com/gif.latex?{0}";

        // IO stuff
        private static TextChannel LatexChannel = new TextChannel("latex", ConsoleColor.Red);

        public static void ConvertEquationAndSendAsync(string code, Channel channel, bool transform)
        {
            if(code == null || channel == null)
            {
                LatexChannel.Write("Código ou canal é nulo!");
                return;
            }

            string url = String.Format(URL_FORMAT, Uri.EscapeUriString(code));
            LatexChannel.Write("Baixando imagem de {0}...", url);

            using (WebClient client = new WebClient())
            {
                client.DownloadDataCompleted += async (sender, e) =>
                {
                    using (MemoryStream convertStream = new MemoryStream(e.Result))
                    {
                        Bitmap bmp = new Bitmap(convertStream); // get image from bytes
                        MemoryStream finalStream;

                        if (transform)
                            finalStream = ImageUtils.GetBitmapStream(ImageUtils.RemoveTransparency(bmp, System.Drawing.Color.Transparent));
                        else
                            finalStream = new MemoryStream(e.Result);

                        await channel.SendFile("naccib.png", finalStream);
                    }                
                };

                client.DownloadDataAsync(new Uri(url));
            }
        }

        private static class ImageUtils
        {
            public static MemoryStream GetBitmapStream(Bitmap bmp)
            {
                ImageConverter converter = new ImageConverter();
                return new MemoryStream((byte[])converter.ConvertTo(bmp, typeof(byte[])));
            }

            public static Bitmap RemoveTransparency(Bitmap bmp, System.Drawing.Color backgroundColor)
            {
                var result = new Bitmap(bmp.Size.Width, bmp.Size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var g = Graphics.FromImage(result);

                g.Clear(backgroundColor);
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.DrawImage(bmp, 0, 0);

                return result;
            }
        }
    }
}
