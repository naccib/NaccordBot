using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Bot
{
    static class Help
    {
        private static readonly Dictionary<string, string> Commands = new Dictionary<string, string>()
        {
            {"help", "Mostra essa mensagem."},
            {"role (role)", "Te coloca em um cargo. Digite `/roles` para ver todos os cargos."},
            {"roles", "Mostra todos os cargos disponíveis para /role." },
            {"google (texto)", "Pesquisa o texto no Google."},
            {"meme (tipo)", "Manda um meme aleatório para o canal atual.\n\nOs tipos podem ser: ```local = Escolhe entre os memes adicionados.\ntwitter = Escolhe um meme entre as contas adicionadas do Twitter```"},
            {"addmeme (link da imagem)", "Adiciona um meme para a lista atual de memes." },
            {"addaccount (nome da conta)", "Adiciona uma conta do Twitter. Ex: `addaccount FilthyFrank`." },
            {"ofender (nome)", "Ofende a pessoa."},
            {"addofensa (ofensa)", "Adiciona uma ofensa. A ofensa deve conter {0}, que será substituido pelo nome da pessoa para ser ofendida." },
            {"cantar (nome)", "Canta a pessoa."},
            {"addcantada (cantada)",  "Adiciona uma cantada. A cantada deve conter {0}, que será substituido pelo nome da pessoa para ser cantada."},
            {"latex (equacao)", "Evalua um código me LaTeX." },
            {"latex2 (equacao)", "Evalua um código me LaTeX com background transparente." },
            {"dump", "Envia dumps para o usuário."},
            {"movies (pesquisa)", "Procura filmes que batem com (pesquisa)." }
        };

        public static string GetCommandString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Comandos são:\n");

            foreach(var kv in Commands)
            {
                sb.AppendFormat("`{0}` -> {1}\n", kv.Key, kv.Value);
            }

            return sb.ToString();
        }
    }
}
