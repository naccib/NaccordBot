using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using Discord;

namespace DiscordBot.Bot
{
    static class Cantadas
    {
        public static string CatchLines
        {
            get
            {
                return Properties.Settings.Default.CatchLines;
            }
            private set
            {
                Properties.Settings.Default.CatchLines = value;
                Properties.Settings.Default.Save();
            }
        }
        private static TextChannel CantadaChannel = new TextChannel("cantada", ConsoleColor.Blue);

        public static void AddCantada(MessageEventArgs e, string cantada)
        {
            if(e.User == null || e == null || e.Channel == null)
            {
                CantadaChannel.Write("Usuário/argumento/canal nulo!");
                return;
            }

            if(!cantada.Contains("{0}"))
            {
                Wrapper.SendMessageAsync(e.User, "Você deve usar no seguinte formato: `{0}, você é top`.");
                CantadaChannel.Write("A cantada não contém {0}.");
                return;
            }

            CatchLines += cantada;
            CatchLines += "\n";

            CantadaChannel.Write("Adicionei cantada de {0}.", e.User.Name);
        }

        public static void Cantar(User recebedor, MessageEventArgs e)
        {
            if (recebedor == null || e == null || e.Channel == null)
            {
                CantadaChannel.Write("Usuário/argumento/canal nulo!");
                return;
            }

            var cantadas = CatchLines.Split('\n').ToList();

            cantadas.Remove(" ");
            cantadas.Remove("");

            if(cantadas.Count == 0)
            {
                CantadaChannel.Write("Sem cantadas.");
                return;
            }

            Wrapper.SendMessageAsync(e.Channel, String.Format(cantadas[new Random().Next(0, cantadas.Count)], recebedor.Mention, e.User.Mention));
        }
    }
}
