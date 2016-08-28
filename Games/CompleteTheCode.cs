using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using DiscordBot.Priting;
using DiscordBot.Bot;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;


namespace DiscordBot.Games
{
    class CompleteTheCode
    {
        // priting stuff
        TextChannel CTChannel = new TextChannel("CTC", ConsoleColor.Green);

        // discord stuff
        private Server CurrentServer;
        private Channel CurrentChannel;

        // players stuff
        private List<User> Players = new List<User>();
        private int CurrentPlayerIndex = 0;
        private User Programmer { get { return Players[CurrentPlayerIndex]; } }

        // game stuff
        private Challenge CurrentChallenge;
        private string CodeString = "";
        private bool DoneChallenge = false;


        public CompleteTheCode(Server sv, Channel ch)
        {
            if (sv == null || ch == null)
                throw new NullReferenceException("Client, Server or Channel is null.");

            CurrentServer = sv;
            CurrentChannel = ch;

            CTChannel.Write("Created a CTC instance.");
            StartGame();
        }

        public CompleteTheCode(MessageEventArgs args)
        {
            if(args == null)
                throw new NullReferenceException("Args is null.");

            CurrentServer = args.Server;
            CurrentChannel = args.Channel;

            CTChannel.Write("Created a CTC instance.");
            StartGame();
        }

        public void AddPlayer(User user)
        {
            Players.Add(user);
            CTChannel.Write("Adicionei o usuário {0}.", user.Name);
        }

        private void StartGame()
        {
            Challenges.Initialize();
            PythonChecker.Initialize();
            CTChannel.Write("Comecei o jogo.");

            Wrapper.SendMessageAsync(CurrentChannel, "Bem vindo ao jogo **C**omplete **T**he **C**ode, programador pelo " + Wrapper.GetMention("naccib", CurrentChannel) + ".\n\nO source code está no GitHub: https://github.com/naccib/NaccordBot. \nSiga o naccib no Twitter: https://twitter.com/vlwvlwvlwvlwvlw.");
            Wrapper.SendMessageAsync(CurrentChannel, "Para entrar no jogo, digite `/play`.\n\n**OBS**: Só jogue se você souber programar em Python!");

            if(Players.Count == 0)
            {
                Wrapper.SendMessageAsync(CurrentChannel, "Não há jogadores! Digite `/play` para jogar!");
                RunPlayerCheckage();
                return;
            }

            RunRound();
        }

        private void RunRound()
        {
            CTChannel.Write("Comecei um novo round.");

            Task roundTask = new Task(async () =>
            {
                CurrentChallenge = Challenges.Random();
                CTChannel.Write("Escolhi a challenge {0}.", CurrentChallenge.Name);
                PythonChecker.SetInput(CurrentChallenge.Input);

                Wrapper.SendMessageAsync(CurrentChannel, String.Format("A challenge atual é **{0}**!\n\nO input é: `{1}` e o output deve ser ```c\n{2}```.", CurrentChallenge.Name, CurrentChallenge.Input == "" ? "vazio" : CurrentChallenge.Input, CurrentChallenge.Output));
                await Task.Delay(1000);

                GetPlayer();

                Wrapper.SendMessageAsync(CurrentChannel, "O programador atual é: " + Programmer.Mention);

                while(!DoneChallenge)
                {
                    await Task.Delay(200);
                }

                // end round, challenge is done.
            });

            roundTask.Start();
        }

        private void GetPlayer()
        {
            if (CurrentPlayerIndex + 1 == Players.Count)
                CurrentPlayerIndex = 0;
            else
                CurrentPlayerIndex++;
        }

        public void AddCode(string code, User e)
        {
            if (e.Id != Programmer.Id)
            {
                CTChannel.Write("{0} tentou escolher pelo programador.", e.Name);
                return;
            }

            CodeString += code;
            CTChannel.Write("Code adicionado por {0}.", e.Name);
            CheckOutput();
        }

        private void RunPlayerCheckage()
        {
            Task checkTask = new Task(async () =>
            {
                while(Players.Count == 0)
                {
                    await Task.Delay(100);
                }

                RunRound();
            });

            checkTask.Start();
        }

        private void CheckOutput()
        {
            CTChannel.Write("Checando output...");
            string output = PythonChecker.RunCode(CodeString);

            if (CurrentChallenge.Output == output)
            {
                CTChannel.Write("Code passed!");
                DoneChallenge = true;
            }
            else
            {
                CTChannel.Write("Output errado: {0}, deveria ser {1}.", output == "" ? "vazio" : output, CurrentChallenge.Output);
                Wrapper.SendMessageAsync(CurrentChannel, "O output foi ```" + output + "``` quando deveria ser ```" + CurrentChallenge.Output + "```");
                GetPlayer(); // atualizar o player
                Wrapper.SendMessageAsync(CurrentChannel, "Agora é a vez de " + Programmer.Mention);
            }
        }
    }

    static class PythonChecker
    {
        private static bool Initialized = false;
        private static ScriptEngine Engine;
        private static ScriptScope Scope;

        public static void Initialize()
        {
            if (Initialized) return;

            Engine = Python.CreateEngine();
            Scope = Engine.CreateScope();

            Scope.SetVariable("output", "");
        }

        public static string RunCode(string code)
        {
            Engine.Execute(code, Scope);

            dynamic output;

            if (Engine.CreateScope().TryGetVariable("output", out output))
                return "Output não existe :[";
            else
                return ((object)output).ToString();
        }

        public static void SetInput(string input)
        {
            Scope.SetVariable("input", input);
        }
    }
}
