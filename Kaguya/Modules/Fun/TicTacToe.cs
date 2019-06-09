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
using System.Collections.Generic;

namespace Kaguya.Modules.Fun
{
    public class TicTacToe : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [RequireContext(ContextType.Guild)]
        [Command("tictactoe", RunMode = RunMode.Async)]
        [Alias("ttt")]
        public async Task TicTacToeGame(IGuildUser player2 = null)
        {
            PlayChoice[,] grid = new PlayChoice[3, 3] {
                { PlayChoice.empty, PlayChoice.empty, PlayChoice.empty },
                { PlayChoice.empty, PlayChoice.empty, PlayChoice.empty},
                { PlayChoice.empty, PlayChoice.empty, PlayChoice.empty } };
            string[] playChoices = new string[9] { "1a", "2a", "3a", "1b", "2b", "3b", "1c", "2c", "3c" }; //TicTacToe play choices.

            bool gameInProgress = true;

            embed.WithTitle($"🎮 Tic Tac Toe");
            embed.WithDescription($"A new game of Tic Tac Toe has started between {Context.User.Mention} and {player2.Mention}!");
            await BE();

            do
            {
                embed.WithDescription($"{Context.User.Mention} It is your turn to play! You have 30 seconds to make your selection!");
                await BE();

                bool p1Repeat = true;
                bool p2Repeat = true;
                bool p1Turn = true;
                bool p2Turn = true;

                do //If player 1 makes a bad selection, keep repeating until they get it right.
                {
                    var difference = DateTime.Now + TimeSpan.FromSeconds(30);
                    SocketMessage p1Ans = await Interactive.NextMessageAsync(Context, false, timeout: TimeSpan.FromSeconds(30));

                    if ((difference - DateTime.Now).TotalSeconds < 0)
                    {
                        //Ends the game if the player takes too long to make a selection.
                        embed.WithDescription($"{Context.User.Mention} You took too long to answer! {player2.Mention} has won the game!");
                        await BE();
                        gameInProgress = false;
                        break;
                    }

                    if (!playChoices.Any(p1Ans.Content.ToLower().Contains) && p1Ans.Author.Id == Context.User.Id)
                    {
                        p1Repeat = true;
                    }
                    else if (playChoices.Any(p1Ans.Content.ToLower().Contains) && p1Ans.Author.Id == Context.User.Id && !selectedTiles.Contains(p1Ans.Content.ToLower()))
                    {
                        //Store choice in play board.

                        grid = AnswerSwitch(grid, p1Ans, PlayChoice.x);
                        selectedTiles.Add(p1Ans.Content.ToLower());

                        //To get the ¯ character, use alt code: ALT+0175.

                        string playString =
                            $"**3** {GetString(grid[0, 2])} | {GetString(grid[1, 2])} | {GetString(grid[2, 2])}\n" +
                            $"**2** {GetString(grid[0, 1])} | {GetString(grid[1, 1])} | {GetString(grid[2, 1])}\n" +
                            $"**1** {GetString(grid[0, 0])} | {GetString(grid[1, 0])} | {GetString(grid[2, 0])}\n" +
                            $"¯¯¯**A**¯¯¯¯**B**¯¯¯¯**C**¯¯";

                        //Checks for any potential winning matches.

                        if (HasAnyRow(grid, PlayChoice.x) || HasAnyColumn(grid, PlayChoice.x) || HasAnyDiagonal(grid, PlayChoice.x))
                        {
                            embed.WithDescription($"{Context.User.Mention} has won the game!" +
                                $"\n" +
                                $"\nFinal Play Area:" +
                                $"\n" +
                                $"\n{playString}");
                            await BE();
                            gameInProgress = false;
                            return;
                        }

                        if (selectedTiles.Count == 9)
                        {
                            //Checks for a Tie.
                            embed.WithDescription($"It's a tie!" +
                                $"\n" +
                                $"\nFinal Play Area:" +
                                $"\n" +
                                $"\n{playString}");
                            await BE();
                            gameInProgress = false;
                            return;
                        }

                        embed.WithDescription($"New play area: \n{playString}");
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
                    var timeout = DateTime.Now + TimeSpan.FromSeconds(30);
                    SocketMessage p2Ans = await Interactive.NextMessageAsync(Context, false, timeout: TimeSpan.FromSeconds(30));

                    if ((timeout - DateTime.Now).TotalSeconds < 0)
                    {
                        //Ends the game if the player takes too long to make a selection.
                        embed.WithDescription($"{player2.Mention} You took too long to answer! {Context.User.Mention} has won the game!");
                        await BE();
                        gameInProgress = false;
                        break;
                    }

                    if (!playChoices.Any(p2Ans.Content.ToLower().Contains) && p2Ans.Author.Id == player2.Id)
                    {
                        p2Repeat = true;
                    }
                    else if (playChoices.Any(p2Ans.Content.ToLower().Contains) && p2Ans.Author.Id == player2.Id && !selectedTiles.Contains(p2Ans.Content.ToLower()))
                    {
                        //Store choice in play board.
                        selectedTiles.Add(p2Ans.Content.ToLower());
                        grid = AnswerSwitch(grid, p2Ans, PlayChoice.o);

                        string playString =
                            $"**3** {GetString(grid[0, 2])} | {GetString(grid[1, 2])} | {GetString(grid[2, 2])}\n" +
                            $"**2** {GetString(grid[0, 1])} | {GetString(grid[1, 1])} | {GetString(grid[2, 1])}\n" +
                            $"**1** {GetString(grid[0, 0])} | {GetString(grid[1, 0])} | {GetString(grid[2, 0])}\n" +
                            $"¯¯¯**A**¯¯¯¯**B**¯¯¯¯**C**¯¯";

                        //Checks for a winner.

                        if (HasAnyRow(grid, PlayChoice.o) || HasAnyColumn(grid, PlayChoice.o) || HasAnyDiagonal(grid, PlayChoice.o))
                        {
                            embed.WithDescription($"{player2.Mention} has won the game!" +
                                $"\n" +
                                $"\nFinal Play Area:" +
                                $"\n" +
                                $"\n{playString}");
                            await BE();
                            gameInProgress = false;
                            return;
                        }

                        if (selectedTiles.Count == 9)
                        {
                            //Checks for a Tie.
                            embed.WithDescription($"It's a tie!" +
                                $"\n" +
                                $"\nFinal Play Area:" +
                                $"\n" +
                                $"\n{playString}");
                            await BE();
                            gameInProgress = false;
                            return;
                        }

                        embed.WithDescription($"New play area: \n\n{playString}");
                        await BE();
                        p2Turn = false;
                        p1Turn = true;
                        break;
                    }
                }
                while (p2Repeat && p2Turn);
            }
            while (gameInProgress);
        }

        private List<string> selectedTiles = new List<string>();

        private string GetString(PlayChoice playChoice)
        {
            if (playChoice == PlayChoice.x)
                return "❌";
            if (playChoice == PlayChoice.o)
                return "⭕";
            return "\\_\\_\\_\\_";
        }

        private bool HasAnyDiagonal(PlayChoice[,] grid, PlayChoice playChoice)
        {
            return HasDiagonal(grid, playChoice);
        }

        private bool HasAnyColumn(PlayChoice[,] grid, PlayChoice playChoice)
        {
            return HasColumn(grid, playChoice, 0) || HasColumn(grid, playChoice, 1) || HasColumn(grid, playChoice, 2);
        }

        private bool HasAnyRow(PlayChoice[,] grid, PlayChoice playChoice)
        {
            return HasRow(grid, playChoice, 0) || HasRow(grid, playChoice, 1) || HasRow(grid, playChoice, 2);
        }

        private bool HasRow(PlayChoice[,] grid, PlayChoice playChoice, int rowNum)
        {
            if (grid[0, rowNum] != PlayChoice.empty && grid[1, rowNum] != PlayChoice.empty && grid[2, rowNum] != PlayChoice.empty)
                return grid[0, rowNum] == playChoice && grid[1, rowNum] == playChoice && grid[2, rowNum] == playChoice;
            else return false;
        }

        private bool HasColumn(PlayChoice[,] grid, PlayChoice playChoice, int columnNum)
        {
            if (grid[columnNum, 0] != PlayChoice.empty && grid[columnNum, 1] != PlayChoice.empty && grid[columnNum, 2] != PlayChoice.empty)
                return grid[columnNum, 0] == playChoice && grid[columnNum, 1] == playChoice && grid[columnNum, 2] == playChoice;
            else return false;
        }

        private bool HasDiagonal(PlayChoice[,] grid, PlayChoice playChoice)
        {
            if (grid[0, 0] != PlayChoice.empty && grid[1, 1] != PlayChoice.empty && grid[2, 2] != PlayChoice.empty || grid[0, 2] != PlayChoice.empty && grid[1, 1] != PlayChoice.empty && grid[2, 0] != PlayChoice.empty)
            {
                if (grid[0, 0] == playChoice && grid[1, 1] == playChoice && grid[2, 2] == playChoice || grid[0, 2] == playChoice && grid[1, 1] == playChoice && grid[2, 0] == playChoice)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        private bool HasWin(PlayChoice[,] grid, PlayChoice playChoice)
        {
            return HasAnyRow(grid, playChoice) || HasAnyColumn(grid, playChoice) || HasAnyDiagonal(grid, playChoice);
        }

        private PlayChoice[,] AnswerSwitch(PlayChoice[,] playArea, SocketMessage answer, PlayChoice playChoice)
        {
            switch (answer.Content.ToLower())
            {
                case "1a":
                    playArea[0, 0] = playChoice; break;
                case "2a":
                    playArea[0, 1] = playChoice; break;
                case "3a":
                    playArea[0, 2] = playChoice; break;
                case "1b":
                    playArea[1, 0] = playChoice; break;
                case "2b":
                    playArea[1, 1] = playChoice; break;
                case "3b":
                    playArea[1, 2] = playChoice; break;
                case "1c":
                    playArea[2, 0] = playChoice; break;
                case "2c":
                    playArea[2, 1] = playChoice; break;
                case "3c":
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

    }
}
