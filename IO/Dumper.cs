using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using DiscordBot.Priting;
using Discord;

namespace DiscordBot.IO
{
    static class Dumper
    {
        // IO stuff
        private static TextChannel DumpChannel = new TextChannel("dump", ConsoleColor.Green);

        // File stuff
        private const string DUMP_FOLDER = "dumps";

        private static bool Initialized = false;

        private static void Initialize()
        {
            if (Initialized) return;

            if (!Directory.Exists(DUMP_FOLDER))
                Directory.CreateDirectory(DUMP_FOLDER);

            Initialized = true;
        }

        public static void SendDump(User user, string commandDump)
        {
            Initialize();

            if(user == null)
            {
                DumpChannel.Write("Tentei mandar um dump para um usuário nulo.");
                return;
            }

            StringBuilder dump = new StringBuilder();

            dump.AppendLine(OtherDumps.CreatorDump());

            dump.AppendLine(commandDump);

            dump.AppendLine(Meme.GetDump());

            DumpChannel.Write("Tarefa iniciada.");
            CreateAndSendDumpAsync(dump.ToString(), user);
        }

        private static string CreateAndSendDumpAsync(string content, User sendTo)
        {
            string tempFileName = String.Format("{0}.txt", Guid.NewGuid().ToString());
            string tempFilePath = Path.Combine(DUMP_FOLDER, tempFileName);

            DumpChannel.Write("Dumpando {0} linhas para {1}.", content.Split('\n').Length, tempFilePath);

            Task t = new Task(async () =>
            {
                using (StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    writer.Write(content);
                    DumpChannel.Write("Escrevi em {0}.", tempFilePath);
                }


                DumpChannel.Write("Mandando {0} para {1}.", tempFilePath, sendTo.Name);
                await sendTo.SendFile(tempFilePath);
            });

            t.Start();
            
            return tempFilePath;
        }
    }

    static class OtherDumps
    {
        public static string CreatorDump()
        {
            StringBuilder dump = new StringBuilder();

            dump.AppendLine("Esse bot foi criado pelo naccib!");
            dump.AppendLine("Siga ele nas redes sociais:");
            dump.AppendLine("  --> https://twitter.com/vlwvlwvlwvlwvlw");
            dump.AppendLine("  --> https://github.com/naccib/NaccordBot (FIND MY SOURCE HERE!)");

            return dump.ToString();
        }
    }
}
