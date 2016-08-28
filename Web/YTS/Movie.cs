using System;

namespace DiscordBot.Web.YTS
{
    public class Movie
    {
        public uint id;
        public string url;
        public string title_long;
        public string slug;
        public string imdb_code;
        public string title;
        public string title_english;
        public int year;
        public float rating;
        public int runtime;
        public string[] genres;
        public string summary;
        public string description_full;
        public string yt_trailer_code;
        public string language;
        public string mpa_rating;
        public string background_image;
        public string background_image_original;
        public string small_cover_image;
        public string medium_cover_image;
        public string large_cover_image;
        public string state;
        public DateTime date_uploaded;
        public ulong date_uploaded_unix;
        public ulong size_bytes;
    }
}
