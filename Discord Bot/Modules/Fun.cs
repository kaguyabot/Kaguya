using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core.UserAccounts;
using System.Net;
using System.Timers;
using Discord_Bot.Core.Server_Files;
using Discord_Bot.Core.Commands;

#pragma warning disable

namespace Discord_Bot.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();

        Color Pink = new Color(252, 132, 255);

        Color Red = new Color(255, 0, 0);

        Color Gold = new Color(255, 223, 0);

        Color Violet = new Color(238, 130, 238);

        public BotConfig bot = new BotConfig();

        public string version = Utilities.GetAlert("VERSION");

        public string botToken = Config.bot.token;

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        [Command("echo")] //fun
        public async Task Echo([Remainder]string message = "")
        {
            if (message == "")
            {
                embed.WithTitle("Echo");
                embed.WithDescription($"**{Context.User.Mention} No message specified!**");
                embed.WithColor(Red);
                BE(); return;
            }
            embed.WithTitle("Echo");
            embed.WithDescription(message);
            embed.WithColor(Pink);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("pick")] //fun
        public async Task PickOne([Remainder]string message = "")
        {
            if (message == "")
            {
                embed.WithTitle("Pick: Missing Options!");
                embed.WithDescription($"**{Context.User.Mention} No options specified!**");
                embed.WithColor(Red);
                BE(); return;
            }

            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            embed.WithTitle("Choice for " + Context.User.Username);
            embed.WithDescription(selection);
            embed.WithColor(Pink);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("gblackjack")]
        public async Task BlackJack(int points)
        {
            if (Context.User.Id != 146092837723832320)
                return;
            if(points < 100)
            {
                embed.WithDescription($"**{Context.User.Mention} Your bet must be at least 100 points to play!**");
                embed.WithColor(Red);
                await BE();
                return;
            }

            var server = Servers.GetServer(Context.Guild);
            string cmdPrefix = server.commandPrefix;
            //if (server.BlackJackInProgress == true)
           // {
          //      embed.WithDescription($"**{Context.User.Mention} a game of blackjack is already in progress. You must wait before it is over to play!**");
           //     embed.WithColor(Red);
           //     await BE(); return;
           // }
            server.JoinedUsers = new List<string>();
            server.BlackJackInProgress = true;
            var playerName = Context.User.Username;
            var userAccount = UserAccounts.GetAccount(Context.User);
            userAccount.Points -= (uint)points; //Takes points away from user on successful bet.
            uint uPoints = userAccount.Points;
            var count = server.JoinedUsers.Count;

            string[] playingCards = { "<:2H:562416295870464012>" , "<:2S:562416296277442561>", "<:2C:562416295895760898>", "<:2D:562416296193687576>", //2's
            "<:3H:562416296206270495>", "<:3S:562416295992229922>", "<:3D:562416296244019210>", "<:3C:562416295979778049>", //3's
            "<:4H:562416296235630602>", "<:4S:562416296206139403>", "<:4C:562416296226979860>", "<:4D:562416296269185094>", //4's
            "<:5H:562416295988035585>", "<:5S:562416296298283008>", "<:5D:562416296428568576>", "<:5C:562416295983972363>", //5's
            "<:6H:562416296491483156>", "<:6C:562416295920795660>", "<:6S:562416296512454656>", "<:6D:562416296323579904>", //6's
            "<:7H:562416296382300201>", "<:7C:562416296424112138>", "<:7D:562416296441020438>", "<:7S:562416296671707166>", //7's
            "<:8D:562416296546009094>", "<:8S:562416296713650176>", "<:8C:562416296692547594>", "<:8H:562416296684290068>", //8's
            "<:9S:562416296839610368>", "<:9h:562416296717713449>", "<:9C:562416296558329877>", "<:9D:562416296487157766>", //9's
            "<:10C:562416296827027503>", "<:10D:562416297313304605>", "<:10S:562416297313435668>", "<:10H:562416297309372417>", //10's
            "<:AC:562416297338601492>", "<:AD:562416297296527382>", "<:AS:562419190955376660>", "<:AH:562416297275817994>", //A's
            "<:JC:562416297238069258>", "<:JH:562416297133211668>", "<:JD:562416296935948292>", "<:JS:562416297166635018>", //J's
            "<:KH:562416297347121152>", "<:KD:562416296977760257>", "<:KS:562416297288138752>", "<:KC:562416297279750144>", //K's
            "<:QS:562416403093782529>", "<:QH:562416321917091851>", "<:QC:562418399209324563>", "<:QD:562416297200058378>"};//Q's

            embed.WithDescription($"**Game of Blackjack Started!** To join in, type `{cmdPrefix}bjoin <points to bet>` to join in!");
            embed.WithColor(Pink);
            await BE();

            //await Task.Delay(10000); //10 second wait for other players to $bjoin.

            server.JoinedUsers.Add(playerName); //Adds all users who use $bjoin to a list.

            Random rand = new Random();

            HashSet<int> cards = new HashSet<int>(); //Hashset ensures that there won't be duplicate cards of the same suit. 
                                                    //(i.e. it's now impossible to draw two Aces of Spades)
            foreach(string player in server.JoinedUsers)
            {
                while (cards.Count < (2 + (2 * player.Count())))
                {
                    cards.Add(rand.Next(0, 51)); //picks a random card out of the "deck" (array).
                }
            }

            //List of cards for players 0-5 (Dealer being 0)

            int i1 = cards.ElementAt(0); 
            int i2 = cards.ElementAt(1);
            int i3 = cards.ElementAt(2);
            int i4 = cards.ElementAt(3);
            int i5 = cards.ElementAt(4);
            int i6 = cards.ElementAt(5);
            int i7 = cards.ElementAt(6);
            int i8 = cards.ElementAt(7);
            int i9 = cards.ElementAt(8);
            int i10 = cards.ElementAt(9);
            int i11 = cards.ElementAt(10);
            int i12 = cards.ElementAt(11);

            string dealerHand1 = playingCards[i1];
            string dealerHand2 = playingCards[i2];
            string player1Hand1 = playingCards[i3];
            string player1Hand2 = playingCards[i4];
            string player2Hand1 = playingCards[i5];
            string player2Hand2 = playingCards[i6];
            string player3Hand1 = playingCards[i7];
            string player3Hand2 = playingCards[i8];
            string player4Hand1 = playingCards[i9];
            string player4Hand2 = playingCards[i10];
            string player5Hand1 = playingCards[i11];
            string player5Hand2 = playingCards[i12];

            DealerHandValues(ref i1, ref i2);
            int dealerHand = i1 + i2;
            //Builds an embed for both of the dealer's cards asynchronously.
            embed.AddField("Dealer's Hand", $"{dealerHand1} {dealerHand2}", true); 
            embed.WithFooter($"Dealer's Hand: {dealerHand}");
            await BE();

            foreach (string player in server.JoinedUsers)
            {
                Player1HandValues(ref i3, ref i4);
                Player2HandValues(ref i5, ref i6);
                Player3HandValues(ref i7, ref i8);
                Player4HandValues(ref i9, ref i10);
                Player4HandValues(ref i11, ref i12);

                int playerHand1 = i3 + i4;
                int playerHand2 = i5 + i6;
                int playerHand3 = i7 + i8;
                int playerHand4 = i9 + i10;
                int playerHand5 = i11 + i12;

                player.Split('#');

                int i = server.JoinedUsers.Count();

                if(i >= 1)
                {
                    if(i == 1)
                    {
                        await Player1(player1Hand1, player1Hand2, player, playerHand1);
                        await BE();
                    }
                    else if(i == 2)
                    {
                        await Player1(player1Hand1, player1Hand2, player, playerHand1);
                        await Player2(player2Hand1, player2Hand2, player, playerHand2);
                        await BE();
                    }
                    else if (i == 3)
                    {
                        await Player1(player1Hand1, player1Hand2, player, playerHand1);
                        await Player2(player2Hand1, player2Hand2, player, playerHand2);
                        await Player3(player2Hand1, player2Hand2, player, playerHand2);
                        await BE();
                    }
                    else if (i == 4)
                    {
                        await Player1(player1Hand1, player1Hand2, player, playerHand1);
                        await Player2(player2Hand1, player2Hand2, player, playerHand2);
                        await Player3(player2Hand1, player2Hand2, player, playerHand2);
                        await Player4(player2Hand1, player2Hand2, player, playerHand2);
                        await BE();
                    }
                    else if (i == 5)
                    {
                        await Player1(player1Hand1, player1Hand2, player, playerHand1);
                        await Player2(player2Hand1, player2Hand2, player, playerHand2);
                        await Player3(player2Hand1, player2Hand2, player, playerHand2);
                        await Player4(player2Hand1, player2Hand2, player, playerHand2);
                        await Player5(player2Hand1, player2Hand2, player, playerHand2);
                        await BE();
                    }
                }
            }

            if (dealerHand > 21) //This really only works for single player right now, need to focus on point rewarding for multiplayer as well.
            {
                embed.WithTitle("Dealer Bust!");
                embed.WithDescription($"**Congratulations!** The dealer has busted! Payout:");
                foreach(string player in server.JoinedUsers)
                {
                    embed.AddField($"{player}", $"{points}", true); //{points} will probably need to be changed for multiplayer (save points bet in list for recall?)
                }
                embed.WithColor(Gold);
            }

            //Execute below only after all players have hit or stayed

            else if(dealerHand <= 15) //Draws another card (or takes a hit)
            {
                await Task.Delay(3500);
                embed.WithTitle("Dealer's Turn");
                embed.WithDescription("Hmm...");
                embed.WithColor(Violet);
                embed.WithFooter($"Dealer's Hand: {dealerHand}");
                BE();
                await Task.Delay(4000);
                cards.Add(rand.Next(0, 51));
                int newCard = cards.ElementAt(0);
                embed.WithDescription("**Dealer Hits**"); 
                BE();
                await Task.Delay(2000);
                dealerHand = dealerHand + DealerHandValues(ref dealerHand, ref newCard);
                embed.WithDescription("**Dealer's Hit Card**");
                embed.AddField("Dealer's Hand", $"{playingCards[newCard]}");
                embed.WithFooter($"Dealer's Hand: {dealerHand}");
                                                                 //Continue working on this
                                                                 //Continue working on this
                                                                 //Continue working on this
                Task.Delay(1000);
                int dealerHandPostTurn = dealerHand;
                if(dealerHandPostTurn <= 15)
                {
                    embed.WithTitle("Dealer's Turn");
                    int newCard2 = cards.ElementAt(rand.Next(13, 51));
                    Task.Delay(1250);
                    embed.WithDescription("**Dealer Hits**");
                    BE();
                    Task.Delay(1500);
                    embed.WithDescription("**Dealer Card**");
                    embed.WithThumbnailUrl($"{playingCards[newCard2]}");
                    dealerHand = dealerHand + DealerHandValues(ref dealerHand, ref newCard2);
                    embed.WithFooter($"Dealer's Hand: {dealerHand}");
                    BE();
                }
            }

            server.JoinedUsers.Clear();
            server.BlackJackInProgress = false; //last thing to execute
        }

        private async Task Player5(string player5Hand1, string player5Hand2, string player, int playerHand5)
        {
            embed.AddField($"{player}'s Hand", $"{player5Hand1} {player5Hand2}", true);
            await Task.Delay(300);
        }

        private async Task Player4(string player4Hand1, string player4Hand2, string player, int playerHand4)
        {
            embed.AddField($"{player}'s Hand", $"{player4Hand1} {player4Hand2}", true);
            await Task.Delay(300);
        }

        private async Task Player3(string player3Hand1, string player3Hand2, string player, int playerHand3)
        {
            embed.AddField($"{player}'s Hand", $"{player3Hand1} {player3Hand2}", true);
            await Task.Delay(300);
        }

        private async Task Player2(string player2Hand1, string player2Hand2, string player, int playerHand2)
        {
            embed.AddField($"{player}'s Hand", $"{player2Hand1} {player2Hand2}", true);
            await Task.Delay(300);
        }

        private async Task Player1(string player1Hand1, string player1Hand2, string player, int playerHand1)
        {
            embed.AddField($"{player}'s Hand", $"{player1Hand1} {player1Hand2}", true);
            await Task.Delay(300);
        }

        private static void Player1HandValues(ref int i3, ref int i4)
        {
            if (0 <= i3 && i3 <= 3) //2
                i3 = 2;
            if (0 <= i4 && i4 <= 3)
                i4 = 2;
            if (4 <= i3 && i3 <= 7) //3
                i3 = 3;
            if (4 <= i4 && i4 <= 7)
                i4 = 3;
            if (8 <= i3 && i3 <= 11) //4
                i3 = 4;
            if (8 <= i4 && i4 <= 11)
                i4 = 4;
            if (12 <= i3 && i3 <= 15) //5
                i3 = 5;
            if (12 <= i4 && i4 <= 15)
                i4 = 5;
            if (16 <= i3 && i3 <= 19) //6
                i3 = 6;
            if (16 <= i4 && i4 <= 19)
                i4 = 6;
            if (20 <= i3 && i3 <= 23) //7
                i3 = 7;
            if (20 <= i4 && i4 <= 23)
                i4 = 7;
            if (24 <= i3 && i3 <= 27) //8
                i3 = 8;
            if (24 <= i4 && i4 <= 27)
                i4 = 8;
            if (28 <= i3 && i3 <= 31) //9
                i3 = 9;
            if (28 <= i4 && i4 <= 31)
                i4 = 9;
            if (32 <= i3 && i3 <= 35) //10
                i3 = 10;
            if (32 <= i4 && i4 <= 35)
                i4 = 10;
            if (36 <= i3 && i3 <= 39) //Ace
                i3 = 11;
            if (36 <= i4 && i4 <= 39)
                i4 = 11;
            if (40 <= i3 && i3 <= 51) //Face cards
                i3 = 10;
            if (40 <= i4 && i4 <= 51)
                i4 = 10;
        }

        private static void Player2HandValues(ref int i5, ref int i6)
        {
            if (0 <= i5 && i5 <= 3) //2
                i5 = 2;
            if (0 <= i6 && i6 <= 3)
                i6 = 2;
            if (4 <= i5 && i5 <= 7) //3
                i5 = 3;
            if (4 <= i6 && i6 <= 7)
                i6 = 3;
            if (8 <= i5 && i5 <= 11) //4
                i5 = 4;
            if (8 <= i6 && i6 <= 11)
                i6 = 4;
            if (12 <= i5 && i5 <= 15) //5
                i5 = 5;
            if (12 <= i6 && i6 <= 15)
                i6 = 5;
            if (16 <= i5 && i5 <= 19) //6
                i5 = 6;
            if (16 <= i6 && i6 <= 19)
                i6 = 6;
            if (20 <= i5 && i5 <= 23) //7
                i5 = 7;
            if (20 <= i6 && i6 <= 23)
                i6 = 7;
            if (24 <= i5 && i5 <= 27) //8
                i5 = 8;
            if (24 <= i6 && i6 <= 27)
                i6 = 8;
            if (28 <= i5 && i5 <= 31) //9
                i5 = 9;
            if (28 <= i6 && i6 <= 31)
                i6 = 9;
            if (32 <= i5 && i5 <= 35) //10
                i5 = 10;
            if (32 <= i6 && i6 <= 35)
                i6 = 10;
            if (36 <= i5 && i5 <= 39) //Ace
                i5 = 11;
            if (36 <= i6 && i6 <= 39)
                i6 = 11;
            if (40 <= i5 && i5 <= 51) //Face cards
                i5 = 10;
            if (40 <= i6 && i6 <= 51)
                i6 = 10;
        }

        private static void Player3HandValues(ref int i7, ref int i8)
        {
            if (0 <= i7 && i7 <= 3) //2
                i7 = 2;
            if (0 <= i8 && i8 <= 3)
                i8 = 2;
            if (4 <= i7 && i7 <= 7) //3
                i7 = 3;
            if (4 <= i8 && i8 <= 7)
                i8 = 3;
            if (8 <= i7 && i7 <= 11) //4
                i7 = 4;
            if (8 <= i8 && i8 <= 11)
                i8 = 4;
            if (12 <= i7 && i7 <= 15) //5
                i7 = 5;
            if (12 <= i8 && i8 <= 15)
                i8 = 5;
            if (16 <= i7 && i7 <= 19) //6
                i7 = 6;
            if (16 <= i8 && i8 <= 19)
                i8 = 6;
            if (20 <= i7 && i7 <= 23) //7
                i7 = 7;
            if (20 <= i8 && i8 <= 23)
                i8 = 7;
            if (24 <= i7 && i7 <= 27) //8
                i7 = 8;
            if (24 <= i8 && i8 <= 27)
                i8 = 8;
            if (28 <= i7 && i7 <= 31) //9
                i7 = 9;
            if (28 <= i8 && i8 <= 31)
                i8 = 9;
            if (32 <= i7 && i7 <= 35) //10
                i7 = 10;
            if (32 <= i8 && i8 <= 35)
                i8 = 10;
            if (36 <= i7 && i7 <= 39) //Ace
                i7 = 11;
            if (36 <= i8 && i8 <= 39)
                i8 = 11;
            if (40 <= i7 && i7 <= 51) //Face cards
                i7 = 10;
            if (40 <= i8 && i8 <= 51)
                i8 = 10;
        }

        private static void Player4HandValues(ref int i9, ref int i10)
        {
            if (0 <= i9 && i9 <= 3) //2
                i9 = 2;
            if (0 <= i10 && i10 <= 3)
                i10 = 2;
            if (4 <= i9 && i9 <= 7) //3
                i9 = 3;
            if (4 <= i10 && i10 <= 7)
                i10 = 3;
            if (8 <= i9 && i9 <= 11) //4
                i9 = 4;
            if (8 <= i10 && i10 <= 11)
                i10 = 4;
            if (12 <= i9 && i9 <= 15) //5
                i9 = 5;
            if (12 <= i10 && i10 <= 15)
                i10 = 5;
            if (16 <= i9 && i9 <= 19) //6
                i9 = 6;
            if (16 <= i10 && i10 <= 19)
                i10 = 6;
            if (20 <= i9 && i9 <= 23) //7
                i9 = 7;
            if (20 <= i10 && i10 <= 23)
                i10 = 7;
            if (24 <= i9 && i9 <= 27) //8
                i9 = 8;
            if (24 <= i10 && i10 <= 27)
                i10 = 8;
            if (28 <= i9 && i9 <= 31) //9
                i9 = 9;
            if (28 <= i10 && i10 <= 31)
                i10 = 9;
            if (32 <= i9 && i9 <= 35) //10
                i9 = 10;
            if (32 <= i10 && i10 <= 35)
                i10 = 10;
            if (36 <= i9 && i9 <= 39) //Ace
                i9 = 11;
            if (36 <= i10 && i10 <= 39)
                i10 = 11;
            if (40 <= i9 && i9 <= 51) //Face cards
                i9 = 10;
            if (40 <= i10 && i10 <= 51)
                i10 = 10;
        }

        private static void Player5HandValues(ref int i11, ref int i12)
        {
            if (0 <= i11 && i11 <= 3) //2
                i11 = 2;
            if (0 <= i12 && i12 <= 3)
                i12 = 2;
            if (4 <= i11 && i11 <= 7) //3
                i11 = 3;
            if (4 <= i12 && i12 <= 7)
                i12 = 3;
            if (8 <= i11 && i11 <= 11) //4
                i11 = 4;
            if (8 <= i12 && i12 <= 11)
                i12 = 4;
            if (12 <= i11 && i11 <= 15) //5
                i11 = 5;
            if (12 <= i12 && i12 <= 15)
                i12 = 5;
            if (16 <= i11 && i11 <= 19) //6
                i11 = 6;
            if (16 <= i12 && i12 <= 19)
                i12 = 6;
            if (20 <= i11 && i11 <= 23) //7
                i11 = 7;
            if (20 <= i12 && i12 <= 23)
                i12 = 7;
            if (24 <= i11 && i11 <= 27) //8
                i11 = 8;
            if (24 <= i12 && i12 <= 27)
                i12 = 8;
            if (28 <= i11 && i11 <= 31) //9
                i11 = 9;
            if (28 <= i12 && i12 <= 31)
                i12 = 9;
            if (32 <= i11 && i11 <= 35) //10
                i11 = 10;
            if (32 <= i12 && i12 <= 35)
                i12 = 10;
            if (36 <= i11 && i11 <= 39) //Ace
                i11 = 11;
            if (36 <= i12 && i12 <= 39)
                i12 = 11;
            if (40 <= i11 && i11 <= 51) //Face cards
                i11 = 10;
            if (40 <= i12 && i12 <= 51)
                i12 = 10;
        }

        private int DealerHandValues(ref int i1, ref int i2)
        {
            if (0 <= i1 && i1 <= 3) //2
                i1 = 2;
            if (0 <= i2 && i2 <= 3)
                i2 = 2;
            if (4 <= i1 && i1 <= 7) //3
                i1 = 3;
            if (4 <= i2 && i2 <= 7)
                i2 = 3;
            if (8 <= i1 && i1 <= 11) //4
                i1 = 4;
            if (8 <= i2 && i2 <= 11)
                i2 = 4;
            if (12 <= i1 && i1 <= 15) //5
                i1 = 5;
            if (12 <= i2 && i2 <= 15)
                i2 = 5;
            if (16 <= i1 && i1 <= 19) //6
                i1 = 6;
            if (16 <= i2 && i2 <= 19)
                i2 = 6;
            if (20 <= i1 && i1 <= 23) //7
                i1 = 7;
            if (20 <= i2 && i2 <= 23)
                i2 = 7;
            if (24 <= i1 && i1 <= 27) //8
                i1 = 8;
            if (24 <= i2 && i2 <= 27)
                i2 = 8;
            if (28 <= i1 && i1 <= 31) //9
                i1 = 9;
            if (28 <= i2 && i2 <= 31)
                i2 = 9;
            if (32 <= i1 && i1 <= 35) //10
                i1 = 10;
            if (32 <= i2 && i2 <= 35)
                i2 = 10;
            if (36 <= i1 && i1 <= 39) //Ace
                i1 = 11;
            if (36 <= i2 && i2 <= 39)
                i2 = 11;
            if (40 <= i1 && i1 <= 51) //Face cards
                i1 = 10;
            if (40 <= i2 && i2 <= 51)
                i2 = 10;
            return i1;
        }

        //Blackjack ends

        [Command("bjoin")]
        public async Task JoinGame(int points)
        {
            if (points < 100)
            {
                embed.WithDescription($"**{Context.User.Mention} Your bet must be at least 100 points to play!**");
                embed.WithColor(Red);
                await BE();
                return;
            }

            var server = Servers.GetServer(Context.Guild);
            var cmdPrefix = server.commandPrefix;
            if(server.JoinedUsers.Count >= 5 && server.BlackJackInProgress == true)
            {
                embed.WithDescription($"**{Context.User.Mention} The maximum number of players are currently playing! Please wait for their game to finish!**");
                embed.WithColor(Red);
                BE();
            }
            else if (server.BlackJackInProgress == false)
            {
                embed.WithDescription($"**{Context.User.Mention} A game of blackjack is not currently in progress. Start one with `{cmdPrefix}gblackjack`!**");
                embed.WithColor(Red);
                BE();
            }
            else if (server.JoinedUsers.Contains(Context.User.Username))
            {
                embed.WithDescription($"**{Context.User.Mention} You have already joined this game of blackjack!**");
                embed.WithColor(Red);
                BE();
            }
            var playerName = Context.User.Username;
            var userAccount = UserAccounts.GetAccount(Context.User);
            uint uPoints = userAccount.Points;
            
            server.JoinedUsers.Add(Context.User.Username);

            embed.WithDescription($"**{Context.User.Mention} has joined the blackjack game with a bet of {points}!**");
            embed.WithColor(Gold);
            BE();
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
