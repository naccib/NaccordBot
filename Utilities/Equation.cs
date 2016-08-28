using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordBot.Priting;
using info.lundin.math;

namespace DiscordBot.Utilities
{
    static class EquationSolver
    {
        // IO stuff
        private static TextChannel UtilitiesChannel = new TextChannel("utilities", ConsoleColor.Yellow);
        private static bool Initialized = false;

        // equation stuff
        private static ExpressionParser Parser;

        private static void Initialize()
        {
            if (Initialized) return;

            Parser = new ExpressionParser();

            Initialized = true;
        }

        public static double Expression(string expr)
        {
            Initialize();

            if(expr == null)
            {
                UtilitiesChannel.Write("Expressão nula!");
                return 0;
            }

            UtilitiesChannel.Write("Evaluando {0}...", expr);

            return Parser.Parse(expr);
        }

        public static bool Bhaskara(double a, double b, double c, out double x1, out double x2)
        {
            Initialize();

            double delta = Math.Pow(b, 2) - (4 * a * c);

            UtilitiesChannel.Write("Achei DELTA = {0}", delta);

            x1 = 0;
            x2 = 0;

            if(delta < 0)
            {
                UtilitiesChannel.Write("Delta é menor que zero...");
                return false;
            }

            x1 = (-b + Math.Sqrt(delta)) / (2 * a);
            x1 = (-b - Math.Sqrt(delta)) / (2 * a);

            return true;
        }
    }
}
