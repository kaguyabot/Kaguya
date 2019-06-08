using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Embed;
using NekosSharp;
using Discord.Addons.Interactive;
using EmbedType = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Fun
{
    public class Fun : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        readonly NekoClient nekoClient = new NekoClient("Kaguya");

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [Command("tictactoe", RunMode = RunMode.Async)]
        [Alias("ttt")]
        public async Task TicTacToe(IGuildUser player2 = null)
        {
            PlayChoice[,] playArea = new PlayChoice[3, 3] {
                { PlayChoice.empty, PlayChoice.empty, PlayChoice.empty },
                { PlayChoice.empty, PlayChoice.empty, PlayChoice.empty},
                { PlayChoice.empty, PlayChoice.empty, PlayChoice.empty } };
            string[] playChoices = new string[9] { "1a", "2a", "3a", "1b", "2b", "3b", "1c", "2c", "3c" }; //TicTacToe play choices.

            bool gameInProgress = true;

            embed.WithTitle($"🎮 Tic Tac Toe");
            embed.WithDescription($"A new game of Tic Tac Toe has started between {Context.User.Mention} and {player2.Mention}!");
            await BE();

            do {
                embed.WithDescription($"{Context.User.Mention} It is your turn to play! You have 30 seconds to make your selection!");
                await BE();

                bool p1Repeat = true;
                bool p2Repeat = true;
                bool p1Turn = true;
                bool p2Turn = true;

                do //If player 1 makes a bad selection, keep repeating until they get it right.
                {
                    SocketMessage p1Ans = await Interactive.NextMessageAsync(Context, false, timeout: TimeSpan.FromSeconds(30));
                    if (!playChoices.Any(p1Ans.Content.ToLower().Contains) && p1Ans.Author.Id == Context.User.Id)
                    {
                        p1Repeat = true;
                    }
                    else if (playChoices.Any(p1Ans.Content.ToLower().Contains))
                    {
                        //Store choice in play board.

                        playArea = AnswerSwitch(playArea, p1Ans, PlayChoice.x);

                        string playString = $"{GetString(playArea[0, 0])} | {GetString(playArea[0, 1])} | {GetString(playArea[0, 2])}\n" +
                            $"{GetString(playArea[1, 0])} | {GetString(playArea[1, 1])} | {GetString(playArea[1, 2])}\n" +
                            $"{GetString(playArea[2, 0])} | {GetString(playArea[2, 1])} | {GetString(playArea[2, 2])}";

                        embed.WithDescription($"New play area: \n{playString}");
                        await BE();
                        p1Turn = false;
                        p2Turn = true;
                        break;
                    }
                    else
                    {
                        embed.WithDescription("I failed for an unknown reason! If this is a repeated issue, please join the support server and contact Stage! Otherwise, please start a new game.");
                        await BE();
                        p1Turn = false;
                        p2Turn = true;
                        break;
                    }
                }
                while (p1Repeat && p1Turn);

                embed.WithDescription($"{player2.Mention} It is your turn to play! You have 30 seconds to make your selection!");
                await BE();

                do //If player 2 makes a bad selection, keep repeating until they get it right.
                {
                    SocketMessage p2Ans = await Interactive.NextMessageAsync(Context, false, timeout: TimeSpan.FromSeconds(30));
                    if (!playChoices.Any(p2Ans.Content.ToLower().Contains) && p2Ans.Author.Id == player2.Id)
                    {
                        p2Repeat = true;
                    }
                    else if (playChoices.Any(p2Ans.Content.ToLower().Contains) && p2Ans.Author.Id == player2.Id)
                    {
                        //Store choice in play board.

                        playArea = AnswerSwitch(playArea, p2Ans, PlayChoice.o);

                        string playString = $"{GetString(playArea[0, 0])} | {GetString(playArea[0, 1])} | {GetString(playArea[0, 2])}\n" +
                            $"{GetString(playArea[1, 0])} | {GetString(playArea[1, 1])} | {GetString(playArea[1, 2])}\n" +
                            $"{GetString(playArea[2, 0])} | {GetString(playArea[2, 1])} | {GetString(playArea[2, 2])}";

                        embed.WithDescription($"New play area: \n{playString}");
                        await BE();
                        p2Turn = false;
                        p1Turn = true;
                        break;
                    }
                    else
                    {
                        embed.WithDescription("I failed for an unknown reason! If this is a repeated issue, please join the support server and contact Stage! Otherwise, please start a new game.");
                        await BE();
                        p2Turn = false;
                        p1Turn = true;
                        break;
                    }
                }
                while (p2Repeat && p2Turn);
            }
            //Code for end game logic
            while (gameInProgress);
        }

        private string GetString(PlayChoice playChoice)
        {
            if (playChoice == PlayChoice.x)
                return "❌";
            if (playChoice == PlayChoice.o)
                return "⭕";
            return "\\_\\_\\_\\_";
        }

        private PlayChoice[,] AnswerSwitch(PlayChoice[,] playArea, SocketMessage answer, PlayChoice playChoice)
        {
            switch (answer.Content.ToLower())
            {
                case "1a":
                case "a1":
                    playArea[0, 0] = playChoice; break;
                case "2a":
                case "a2":
                    playArea[0, 1] = playChoice; break;
                case "3a":
                case "a3":
                    playArea[0, 2] = playChoice; break;
                case "1b":
                case "b1":
                    playArea[1, 0] = playChoice; break;
                case "2b":
                case "b2":
                    playArea[1, 1] = playChoice; break;
                case "3b":
                case "b3":
                    playArea[1, 2] = playChoice; break;
                case "1c":
                case "c1":
                    playArea[2, 0] = playChoice; break;
                case "2c":
                case "c2":
                    playArea[2, 1] = playChoice; break;
                case "3c":
                case "c3":
                    playArea[2, 2] = playChoice; break;
            }
            return playArea;
        }

        enum PlayChoice //TicTacToe play choices
        {
            x,
            o,
            empty
        }

        [Command("fact")]
        public async Task RandomFact()
        {
            var factsFile = File.ReadAllLines("Resources/Facts.txt");
            Random rng = new Random();
            int rngNum = rng.Next(factsFile.Count());
            string fact = factsFile.ElementAt(rngNum);

            embed.WithTitle($"Random Fact #{rngNum}");
            embed.WithDescription(fact);
            await BE();
        }

        [Command("echo")] //fun
        public async Task Echo([Remainder]string message = "")
        {
            var filteredWords = Servers.GetServer(Context.Guild).FilteredWords;

            if (message == "")
            {
                embed.WithDescription($"**{Context.User.Mention} No message specified!**");
                await BE(); return;
            }

            foreach(var word in filteredWords)
            {
                if (message.Contains(word))
                    return;
            }

            embed.WithDescription(message);
            await BE();
        }

        [Command("pick")] //fun
        public async Task PickOne([Remainder]string message = "")
        {
            if (message == "")
            {
                embed.WithTitle("Pick: Missing Options!");
                embed.WithDescription($"**{Context.User.Mention} No options specified!**");
                await BE();
            }

            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            embed.WithTitle("Choice for " + Context.User.Username);
            embed.WithDescription(selection);

            await BE();
        }

        [Command("8ball")]
        public async Task EightBall([Remainder]string question)
        {
            string filePath = "Resources/8ball.txt";
            string[] responses = File.ReadAllLines(filePath);
            Random rand = new Random();

            var num = rand.Next(14);

            embed.WithTitle("Magic 8Ball");
            embed.WithDescription($"**{Context.User.Mention} {responses[num]}**");
            await BE();
            
        }

        [Command("slap")]
        public async Task Slap(string target)
        {
            var gif = await nekoClient.Action_v3.SlapGif();
            embed.WithTitle($"{Context.User.Username} slaped {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("slap")]
        public async Task Slap(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.SlapGif();
            embed.WithTitle($"{Context.User.Username} slaped {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("hug")]
        public async Task Hug(string target)
        {
            var gif = await nekoClient.Action_v3.HugGif();
            embed.WithTitle($"{Context.User.Username} hugged {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("hug")]
        public async Task Hug(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.HugGif();
            embed.WithTitle($"{Context.User.Username} hugged {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("kiss")]
        public async Task Kiss(string target)
        {
            var gif = await nekoClient.Action_v3.KissGif();
            embed.WithTitle($"{Context.User.Username} kissed {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("kiss")]
        public async Task Kiss(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.KissGif();
            embed.WithTitle($"{Context.User.Username} kissed {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("pat")]
        public async Task Pat(string target)
        {
            var gif = await nekoClient.Action_v3.PatGif();
            embed.WithTitle($"{Context.User.Username} patted {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("pat")]
        public async Task Pat(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.PatGif();
            embed.WithTitle($"{Context.User.Username} patted {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("poke")]
        public async Task Poke(string target)
        {
            var gif = await nekoClient.Action_v3.PokeGif();
            embed.WithTitle($"{Context.User.Username} poked {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("poke")]
        public async Task Poke(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.PokeGif();
            embed.WithTitle($"{Context.User.Username} poked {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("tickle")]
        public async Task Tickle(string target)
        {
            var gif = await nekoClient.Action_v3.TickleGif();
            embed.WithTitle($"{Context.User.Username} tickled {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("tickle")]
        public async Task Tickle(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.TickleGif();
            embed.WithTitle($"{Context.User.Username} tickled {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("baka")]
        public async Task Baka()
        {
            var gif = await nekoClient.Image_v3.BakaGif();
            embed.WithTitle($"Baka!!");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("nekoavatar")]
        public async Task NekoAvatar()
        {
            var gif = await nekoClient.Image_v3.NekoAvatar();
            embed.WithTitle($"Neko Avatar for {Context.User.Username}");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("smug")]
        public async Task Smug()
        {
            var gif = await nekoClient.Image_v3.SmugGif();
            embed.WithTitle($"Smug（￣＾￣）");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("waifu")]
        public async Task Waifu()
        {
            var gif = await nekoClient.Image_v3.Waifu();
            embed.WithTitle($"Waifu (ﾉ≧ڡ≦)");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        [Command("wallpaper")]
        public async Task Wallpaper()
        {
            var gif = await nekoClient.Image_v3.Wallpaper();
            embed.WithTitle($"Wallpaper for {Context.User.Username}");
            embed.WithImageUrl(gif.ImageUrl);
            await BE();
        }

        private bool UserIsAdmin(SocketGuildUser user)
        {
            string targetRoleName = "Administrator";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }
    }
}
