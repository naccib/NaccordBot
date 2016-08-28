using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using Discord;
using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;

namespace DiscordBot.Web
{
    static class GoogleSearch
    {
        private const string API_KEY = "AIzaSyAvu9h2DdFP40Pqi9A8t97NgIAOs64PLc8";
        private const string SEARCH_ENGINE_ID = "016479116295048594496:wlkyfocqjny";
        private static bool Initialized = false;

        // IO stuff
        private static TextChannel GoogleChannel = new TextChannel("google", ConsoleColor.Blue);
        private static CustomsearchService SearchService = null;

        public static void Initialize()
        {
            if (Initialized) return;

            GoogleChannel.Write("Initializing...");

            SearchService = new CustomsearchService(new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = API_KEY });
            GoogleChannel.Write("Created search service with API key {0}.", API_KEY);

            Initialized = true;
        }

        public static void Search(string query, MessageEventArgs e)
        {
            Initialize();
            StringBuilder result = new StringBuilder();

            Task searchTask = new Task(async () =>
            {
                CseResource.ListRequest listRequest = SearchService.Cse.List(query);
                listRequest.Cx = SEARCH_ENGINE_ID;

                Search search = await listRequest.ExecuteAsync();
                GoogleChannel.Write("Found {0} results to {1}.", search.Items.Count, query);

                result.AppendLine("Displaying {0} results:");

                foreach (var item in search.Items)
                {
                    result.AppendFormat("`{0}` - {1}\n", item.Title, item.Link);
                }

                if (e.Channel == null)
                    await e.Channel.SendMessage(result.ToString());
                else
                    await e.User.SendMessage(result.ToString());
            });

            searchTask.Start();
        }

        public static void Search(string query, Channel channel)
        {
            try
            {
                Initialize();
                StringBuilder result = new StringBuilder();

                Task searchTask = new Task(async () =>
                {
                    CseResource.ListRequest listRequest = SearchService.Cse.List(query);
                    listRequest.Cx = SEARCH_ENGINE_ID;

                    Search search = await listRequest.ExecuteAsync();

                    if(search.Items == null)
                    {
                        GoogleChannel.Write("API do Google está sem resposta!");
                        Bot.Wrapper.SendMessageAsync(channel, "Google indisponível no momento.\nTente novamente mais tarde! *biip*");
                        return;
                    }

                    GoogleChannel.Write("Found {0} results to {1}.", search.Items.Count, query);

                    result.AppendFormat("Displaying {0} results:\n", search.Items.Count);

                    foreach (var item in search.Items)
                    {
                        result.AppendFormat("`{0}` - {1}\n", item.Title, item.Link);
                    }

                    await channel.SendMessage(result.ToString());
                });

                searchTask.Start();
            }
            catch(Exception e)
            {
                GoogleChannel.Write("Erro ao pesquisar! {0} : {1}", e.Message, e.Source);
            }
        }
    }
}
