using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using DiscordBot.Priting;
using Discord;
using YoutubeExtractor;

namespace DiscordBot.Web
{
    static class YoutubeAudioDownloader
    {
        // async stuff
        private delegate void DownloadAudioDelegate(string link, MessageEventArgs e);

        // song stuff

        public static List<SongInfo> Songs = new List<SongInfo>();

        public static SongInfo CurrentSong = null;
        private static int CurrentSongIndex = 0;

        // io stuff

        private static TextChannel YTChannel = new TextChannel("youtube", ConsoleColor.Red);

        public static void Initialize()
        {
            ManageSongDownloading();
        }

        public static void DownloadAudio(string link, MessageEventArgs e)
        {
            if(String.IsNullOrEmpty(link))
            {
                YTChannel.Write("Link é nulo!");
                return;
            }
            
            IEnumerable<VideoInfo> videos = DownloadUrlResolver.GetDownloadUrls(link);
            
            if(videos == null || videos.Count() == 0)
            {
                YTChannel.Write("Não consegui nenhum link para {0}.", link);
                return;
            }

            YTChannel.Write("Achei {0} videos para o link {1}.", videos.Count(), link);

            VideoInfo video = videos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

            if(video == null)
            {
                YTChannel.Write("Nenhum dos vídeos encontrados contém audio válido.");
                return;
            }

            YTChannel.Write("Encontrei o vídeo {0}.", video.Title);

            if(!Directory.Exists("temp"))
            {
                YTChannel.Write("Diretório temp não existe, criando um...");
                Directory.CreateDirectory("temp");
            }

            var audioDownloader = new AudioDownloader(video, Path.Combine("temp", video.Title + video.AudioExtension));

            audioDownloader.DownloadStarted += (sender, args) => YTChannel.Write("Comecei a baixar {0}.", video.Title + video.AudioExtension);
            audioDownloader.DownloadFinished += (sender, args) => YTChannel.Write("Termiei de baixar {0}.", video.Title + video.AudioExtension);

            audioDownloader.Execute();

            Songs.Add(new SongInfo(video.Title, audioDownloader.SavePath, e.User.Mention, link, e));
        }

        private static void DownloadAudioAsync(string link, MessageEventArgs e)
        {
            DownloadAudioDelegate caller = new DownloadAudioDelegate(DownloadAudio);
            IAsyncResult result = caller.BeginInvoke(link, e, null, null);

            caller.EndInvoke(result);
        }

        public static void PlayNext()
        {
            if (Songs.Count == 0)
            {
                YTChannel.Write("There are no songs.");
                return;
            }

            if(CurrentSong == null)
                CurrentSong = Songs[CurrentSongIndex];
        }

        private static void ManageSongDownloading()
        {
            Task manageTask = new Task(async () =>
            {
                while(true)
                {
                    if(Songs.Count > 2)
                    {
                        DownloadAudioAsync(Songs[0].Url, Songs[0].Message);
                    }

                    await Task.Delay(100);
                }
            });
            manageTask.Start();
        }
    }

    public class SongInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string WhoAdded { get; set; }
        public string Url { get; set; }
        public MessageEventArgs Message;

        public SongInfo(string name, string path, string whoadded, string url, MessageEventArgs args)
        {
            Name = name;
            Path = path;
            WhoAdded = whoadded;
            Url = url;
            Message = args;
        }
    }
}
