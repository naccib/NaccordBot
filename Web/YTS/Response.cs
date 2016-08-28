namespace DiscordBot.Web.YTS
{
    public class Response
    {
        public string status;
        public string status_message;
        public Data data;
    }

    public class Data
    {
        public int movie_count;
        public int limit;
        public int page_number;
        public Movie[] movies;
    }
}
