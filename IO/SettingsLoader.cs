using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using DiscordBot.Properties;
using DiscordBot.Priting;
using Newtonsoft.Json;

namespace DiscordBot.IO
{
    static class SettingsLoader
    {
        public const string SETTINGS_FILE_PATH = "settings.ini";
        public static Settings _Settings;

        private static TextChannel IOChannel = new TextChannel("io", ConsoleColor.Green, ConsoleColor.White);

        public static string Token
        {
            get;
            private set;
        }
        public static List<ulong> IgnoredUsers;

        public static void Initialize()
        {
            IOChannel.Write("Inicializando IO...");

            if (!File.Exists(SETTINGS_FILE_PATH))
            {
                File.Create(SETTINGS_FILE_PATH);
                IOChannel.Write("Não consegui achar o arquivo \"{0}\", criando um novo...", SETTINGS_FILE_PATH);
            }

            string content = File.ReadAllText(SETTINGS_FILE_PATH);
            Settings loaded = JsonConvert.DeserializeObject<Settings>(content);

            Token = loaded.Token ?? "invalid_token";
            IgnoredUsers = loaded.Ignored.ToList();

            if (Token != "invalid_token")
                IOChannel.Write("Achei a token {0}.", Token);
            else
                IOChannel.Write("Não achei a token! Leia readme.txt para mais informação.");

            DiscordBot.Properties.Settings.Default.Token = Token;
            DiscordBot.Properties.Settings.Default.StatusLocation = loaded.DiscordStatusServer;

            DiscordBot.Properties.Settings.Default.Save();

            _Settings = loaded;
        }

        public static void End()
        {
            File.WriteAllText(SETTINGS_FILE_PATH, JsonConvert.SerializeObject(_Settings));

            IOChannel.Write("Escrevi em ", SETTINGS_FILE_PATH);
        }

        public class Settings
        {
            public string Token = "invalid_token";
            public ulong[] Ignored;
            public string DiscordStatusServer = "https://discord.statuspage.io";
        }
    }
}
