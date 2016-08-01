using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using DiscordBot.IO;
using DiscordBot.Bot;
using System.Runtime.InteropServices;

namespace DiscordBot
{
    class Program
    {
        static TextChannel mainCh = new TextChannel("main", ConsoleColor.Cyan, ConsoleColor.White);

        static void Main(string[] args)
        {
            mainCh.Write("Começando o Bot...");

            SettingsLoader.Initialize();
            CommandDictionary.Initialize();
            Wrapper.Initialize();

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            Console.Read();
        }

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                mainCh.Write("Limpando tudo...");

                Wrapper.UpdateSettings();
                SettingsLoader.End();
                Games.CAH.DeleteChannel();
                Games.CAH.ResetRole();

                System.Threading.Thread.Sleep(2000);
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }
}
