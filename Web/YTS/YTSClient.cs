using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

using DiscordBot.Priting;
using DiscordBot.Bot;
using System.IO;

namespace DiscordBot.Web.YTS
{
    static class YTSClient
    {
        // web stuff
        private const string LIST_MOVIES_QUERY_FORMAT = "https://yts.ag/api/v2/list_movies.json?query_term={0}&limit=20";

        // io stuff
        private static TextChannel MoviesChannel = new TextChannel("movies", ConsoleColor.Magenta);

        public static Movie[] GetMovies(string query)
        {
            if(query == null)
            {
                MoviesChannel.Write("Query nula.");
                return null;
            }

            string url = String.Format(LIST_MOVIES_QUERY_FORMAT, query);

            using (WebClient client = new WebClient())
            {
                byte[] data = client.DownloadData(url);
                string json = Encoding.UTF8.GetString(data);

                MoviesChannel.Write("Baixei {0} bytes com {1} caractéres.", data.Length, json.Length);
                Response response = JsonConvert.DeserializeObject<Response>(json);

                return response.data.movies;
            }
        }

        public static async Task<Movie[]> GetMoviesAsync(string query)
        {
            if (query == null)
            {
                MoviesChannel.Write("Query nula.");
                return null;
            }

            string url = String.Format(LIST_MOVIES_QUERY_FORMAT, query);

            using (WebClient client = new WebClient())
            {
                byte[] data = await client.DownloadDataTaskAsync(url);
                string json = Encoding.UTF8.GetString(data);

                MoviesChannel.Write("Baixei {0} bytes com {1} caractéres.", data.Length, json.Length);
                Response response = JsonConvert.DeserializeObject<Response>(json);

                return response.data.movies;
            }
        }

        public static void SendMovies(Movie[] movies, Discord.Channel channel)
        {
            string result = "";

            Func<string, object, object> getPair = (a, b) =>
            {
                return String.Format("{0}: {1}\n", a, b);
            };

            Func<string[], string> getGenres = (a) =>
            {
                string _res = "```";

                foreach (string str in a)
                    _res += str + "\n";

                _res += "```";

                return _res;
            };

            foreach(Movie m in movies)
            {
                result += String.Format("[{0}]\n", m.title);

                result += getPair("**Title**", m.title_long);
                result += getPair("**Year**", m.year);
                result += getPair("**Rating**", m.mpa_rating);
                result += getPair("**Genres**", getGenres(m.genres));
                result += getPair("**URL**", m.url);

                result += getPair("**Image**", m.medium_cover_image);
                result += "\n\n";
            }

            string[] messages = SplitMessage(result, 1999);

            foreach (string msg in messages)
                Wrapper.SendMessageAsync(channel, msg);
        }

        public static string[] SplitMessage(string message, int chunkSize)
        {
            if (chunkSize < 1)
                chunkSize = 1;

            if (message == null)
                return new string[] { "" };

            if (message.Length <= chunkSize)
                return new string[] { message };

            string[] result = new string[Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(message.Length / chunkSize)))];
            MoviesChannel.Write("Separando {0} caracteres em {1} partes.", message.Length, result.Length);

            for(int i = 0; i < result.Length; ++i)
            {
                result[i] = message.Substring(i * chunkSize, chunkSize);
            }

            return result;
        }
    }
}
