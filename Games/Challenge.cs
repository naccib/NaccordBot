using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Games
{
    public class Challenge
    {
        public string Name { get; set; }
        public string Output { get; set; }
        public string Input { get; set; }
    }

    public static class Challenges
    {
        private static List<Challenge> AllChallenges = new List<Challenge>();
        private static bool Initialized = false;

        public static void Initialize()
        {
            if (Initialized) return;

            // add all challenges

            AllChallenges.Add(new Challenge()
            {
                Name = "Hello, World!",
                Output = "Hello, World!",
                Input = ""
            });

            AllChallenges.Add(new Challenge()
            {
                Name = "Add 2",
                Output = "4",
                Input = "2"
            });
        }
        
        public static Challenge Random()
        {
            return AllChallenges[new Random().Next(0, AllChallenges.Count - 1)];
        }
    }
}
