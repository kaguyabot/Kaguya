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
using EmbedType = Kaguya.Core.Embed.EmbedType;

namespace Kaguya.Modules
{
    public class Fun : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        readonly NekoClient nekoClient = new NekoClient("Kaguya");


        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
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
                //logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User did not specify any options to pick from."); return;
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
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("slap")]
        public async Task Slap(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.SlapGif();
            embed.WithTitle($"{Context.User.Username} slaped {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("hug")]
        public async Task Hug(string target)
        {
            var gif = await nekoClient.Action_v3.HugGif();
            embed.WithTitle($"{Context.User.Username} hugged {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("hug")]
        public async Task Hug(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.HugGif();
            embed.WithTitle($"{Context.User.Username} hugged {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("kiss")]
        public async Task Kiss(string target)
        {
            var gif = await nekoClient.Action_v3.KissGif();
            embed.WithTitle($"{Context.User.Username} kissed {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("kiss")]
        public async Task Kiss(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.KissGif();
            embed.WithTitle($"{Context.User.Username} kissed {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("pat")]
        public async Task Pat(string target)
        {
            var gif = await nekoClient.Action_v3.PatGif();
            embed.WithTitle($"{Context.User.Username} patted {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("pat")]
        public async Task Pat(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.PatGif();
            embed.WithTitle($"{Context.User.Username} patted {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("poke")]
        public async Task Poke(string target)
        {
            var gif = await nekoClient.Action_v3.PokeGif();
            embed.WithTitle($"{Context.User.Username} poked {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("poke")]
        public async Task Poke(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.PokeGif();
            embed.WithTitle($"{Context.User.Username} poked {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("tickle")]
        public async Task Tickle(string target)
        {
            var gif = await nekoClient.Action_v3.TickleGif();
            embed.WithTitle($"{Context.User.Username} tickled {target}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("tickle")]
        public async Task Tickle(IGuildUser target)
        {
            var gif = await nekoClient.Action_v3.TickleGif();
            embed.WithTitle($"{Context.User.Username} tickled {target.Username}!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("baka")]
        public async Task Baka()
        {
            var gif = await nekoClient.Image_v3.BakaGif();
            embed.WithTitle($"Baka!!");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("nekoavatar")]
        public async Task NekoAvatar()
        {
            var gif = await nekoClient.Image_v3.NekoAvatar();
            embed.WithTitle($"Neko Avatar for {Context.User.Username}");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("smug")]
        public async Task Smug()
        {
            var gif = await nekoClient.Image_v3.SmugGif();
            embed.WithTitle($"Smug（￣＾￣）");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("waifu")]
        public async Task Waifu()
        {
            var gif = await nekoClient.Image_v3.Waifu();
            embed.WithTitle($"Waifu (ﾉ≧ڡ≦)");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        [Command("wallpaper")]
        public async Task Wallpaper()
        {
            var gif = await nekoClient.Image_v3.Wallpaper();
            embed.WithTitle($"Wallpaper for {Context.User.Username}");
            embed.WithImageUrl(gif.ImageUrl);
            embed.SetColor(EmbedType.VIOLET);
            await BE();
        }

        //[Command("blackjack1", RunMode = RunMode.Async)]
        //[RequireOwner]
        //public async Task BlackJack(int points)
        //{
        //    if(points < 100)
        //    {
        //        embed.WithDescription($"**{Context.User.Mention} Your bet must be at least 100 points to play!**");
        //        embed.WithColor(Red);
        //        await BE();
        //        return;
        //    }

        //    var server = Servers.GetServer(Context.Guild);
        //    string cmdPrefix = server.commandPrefix;
        //    //if (server.BlackJackInProgress == true) 
        //    //{
        //    //    embed.WithDescription($"**{Context.User.Mention} a game of blackjack is already in progress. You must wait before it is over to play!**");
        //    //    embed.WithColor(Red);
        //    //    await BE(); return;
        //    //}
        //    server.JoinedUsers = new List<string>();
        //    server.BlackJackInProgress = true;
        //    var playerName = Context.User.Username;
        //    var userAccount = UserAccounts.GetAccount(Context.User);
        //    userAccount.Points -= (uint)points; //Takes points away from user on successful bet.
        //    uint uPoints = userAccount.Points;
        //    var count = server.JoinedUsers.Count;

        //    //Emote codes for 52 playing cards

        //    string[] playingCards = { "<:2H:562416295870464012>" , "<:2S:562416296277442561>", "<:2C:562416295895760898>", "<:2D:562416296193687576>", //2's
        //    "<:3H:562416296206270495>", "<:3S:562416295992229922>", "<:3D:562416296244019210>", "<:3C:562416295979778049>", //3's
        //    "<:4H:562416296235630602>", "<:4S:562416296206139403>", "<:4C:562416296226979860>", "<:4D:562416296269185094>", //4's
        //    "<:5H:562416295988035585>", "<:5S:562416296298283008>", "<:5D:562416296428568576>", "<:5C:562416295983972363>", //5's
        //    "<:6H:562416296491483156>", "<:6C:562416295920795660>", "<:6S:562416296512454656>", "<:6D:562416296323579904>", //6's
        //    "<:7H:562416296382300201>", "<:7C:562416296424112138>", "<:7D:562416296441020438>", "<:7S:562416296671707166>", //7's
        //    "<:8D:562416296546009094>", "<:8S:562416296713650176>", "<:8C:562416296692547594>", "<:8H:562416296684290068>", //8's
        //    "<:9S:562416296839610368>", "<:9h:562416296717713449>", "<:9C:562416296558329877>", "<:9D:562416296487157766>", //9's
        //    "<:10C:562416296827027503>", "<:10D:562416297313304605>", "<:10S:562416297313435668>", "<:10H:562416297309372417>", //10's
        //    "<:AC:562416297338601492>", "<:AD:562416297296527382>", "<:AS:562419190955376660>", "<:AH:562416297275817994>", //A's
        //    "<:JC:562416297238069258>", "<:JH:562416297133211668>", "<:JD:562416296935948292>", "<:JS:562416297166635018>", //J's
        //    "<:KH:562416297347121152>", "<:KD:562416296977760257>", "<:KS:562416297288138752>", "<:KC:562416297279750144>", //K's
        //    "<:QS:562416403093782529>", "<:QH:562416321917091851>", "<:QC:562418399209324563>", "<:QD:562416297200058378>"};//Q's

        //    //await Task.Delay(10000); //10 second wait for other players to $bjoin.

        //    server.JoinedUsers.Add(playerName); //Adds all users who use $bjoin to a list.

        //    Random rand = new Random();

        //    HashSet<int> cards = new HashSet<int>(); //Hashset ensures that there won't be duplicate cards of the same suit. 
        //                                            //(i.e. it's now impossible to draw two Aces of Spades)
        //    for (int i = 0; i < 51; i++)
        //    {
        //        cards.Add(rand.Next(51)); //Adds all cards in array to hashset.
        //    }

        //    int i1 = cards.ElementAt(0); //Dealer's hand first card
        //    int i2 = cards.ElementAt(1); //Dealer's hand second card
        //    int i3 = cards.ElementAt(2); //Player's hand first card
        //    int i4 = cards.ElementAt(3); //Player's hand second card
        //    int i5 = cards.ElementAt(4); //Dealer hit card #1
        //    int i6 = cards.ElementAt(5); //Dealer hit card #2
        //    int i7 = cards.ElementAt(6); //Player's first hit card
        //    int i8 = cards.ElementAt(7); //Player's second hit card

        //    //Below contains array indicies for playing cards.

        //    string dealerHand1 = playingCards[i1]; //Dealer's first card
        //    string dealerHand2 = playingCards[i2]; //Dealer's second card
        //    string player1Hand1 = playingCards[i3]; //Player's first card
        //    string player1Hand2 = playingCards[i4]; //Player's second card
        //    string dealerHitCard1 = playingCards[i5]; //Called when dealer's hand is below 16, 3rd card in hand. After player's turn has ended.
        //    string dealerHitCard2 = playingCards[i6]; //Called when dealer's hand is still below 16, 4th card in hand.
        //    string player1HitCard1 = playingCards[i7];
        //    string player1HitCard2 = playingCards[i8];

        //    HandValues(ref i1, ref i2);
        //    int dealerHand = i1 + i2;
        //    HandValues(ref i3, ref i4);
        //    int playerHand = i3 + i4;
        //    HitCardValue(ref i5);
        //    int hitCard1 = i5;
        //    HitCardValue(ref i6);
        //    int hitCard2 = i6;
        //    HitCardValue(ref i7);
        //    int playerHitCard1 = i7;
        //    HitCardValue(ref i8);
        //    int playerHitCard2 = i8;

        //    //Builds an embed for both of the dealer's cards.
        //    embed.WithTitle("Game of Blackjack Started");
        //    embed.AddField("Dealer's Hand", $"{dealerHand1} {dealerHand2}", true);
        //    embed.AddField($"Your Hand", $"{player1Hand1} {player1Hand2}", true); //Adds an inline field for the player's hand
        //    embed.WithFooter($"Dealer's Hand: {dealerHand} | Your Hand: {playerHand}");
        //    embed.WithColor(Pink);
        //    await BE();

        //    //Code for player hit/split/etc options here

        //    embed.WithTitle("Player's Turn");
        //    embed.WithDescription($"**{Context.User.Mention} Reply with [Hit/Stand/Split] based on what you would like to do!**");
        //    embed.WithColor(Pink);
        //    await BE();

        //    _interactive = Global.Interactive;
        //    var response = await _interactive.NextMessageAsync(Context);

        //    Console.WriteLine("Response: " + response);

        //    if (response == null)
        //    {
        //        Task.Delay(25000);
        //    }
        //    else if (response != null)
        //    {
        //        EmbedBuilder embed4 = new EmbedBuilder();
        //        embed4.WithTitle("Player Hit");
        //        embed4.AddField("Dealer's Hand", $"{dealerHand1} {dealerHand2}", true);
        //        embed4.AddField("Player's Hand", $"{player1Hand1} {player1Hand2} {player1HitCard1}", true);

        //        playerHand = playerHand + i7;

        //        embed4.WithFooter($"Dealer's Hand: {dealerHand} | Player's Hand: {playerHand}");
        //        await Context.Channel.SendMessageAsync("", false, embed4.Build());

        //        server.JoinedUsers.Clear();
        //        server.BlackJackInProgress = false;
        //        return;
        //    }


        //    if(dealerHand < 16) //Draws another card (or takes a hit) when hand is less than 16.
        //    {
        //        EmbedBuilder embed2 = new EmbedBuilder();
        //        embed2.WithTitle("Dealer's Turn");
        //        embed2.WithDescription("**Dealer's Hand Below 16: Hits**");
        //        embed2.AddField("Dealer's New Hand", $"{dealerHand1} {dealerHand2} {dealerHitCard1}", true);
        //        embed2.AddField("Your Hand", $"{player1Hand1} {player1Hand2}", true);
        //        embed2.WithFooter($"Dealer's Hand: {dealerHand + i5} | Your Hand: {playerHand}");
        //        Console.WriteLine("HitCard 1: " + i5);
        //        embed2.WithColor(Pink);
        //        await Context.Channel.SendMessageAsync("", false, embed2.Build());

        //        int dealerHandB = dealerHand + i5;

        //        Task.Delay(500);

        //        if(dealerHand < 16) //Second hit if hand is *still* below 16, dealerHandB goes here after debugging.
        //        {
        //            EmbedBuilder embed3 = new EmbedBuilder();
        //            embed3.WithTitle("Dealer's Turn");
        //            embed3.WithDescription("**Dealer Below 16, Hitting Again**");
        //            embed3.AddField("Dealer's New Hand", $"{dealerHand1} {dealerHand2} {dealerHitCard1} {dealerHitCard2}", true);
        //            embed3.AddField("Your Hand", $"{player1Hand1} {player1Hand2}", true);
        //            embed3.WithFooter($"Dealer's Hand: {dealerHandB + i6} | Your Hand: {playerHand}");
        //            embed3.WithColor(Pink);
        //            await Context.Channel.SendMessageAsync("", false, embed3.Build());

        //        }
        //    }

        //    if (dealerHand > 21) //When dealer hand is > 21, bust and award player
        //    {
        //        embed.WithTitle("Dealer Bust!");
        //        embed.WithDescription($"**Congratulations!** The dealer has busted! Payout:");
        //        foreach (string player in server.JoinedUsers)
        //        {
        //            embed.AddField($"{player}", $"{points}", true); //{points} will probably need to be changed for multiplayer (save points bet in list for recall?)
        //        }
        //        embed.WithColor(Gold);
        //        await BE();
        //        server.JoinedUsers.Clear();
        //        server.BlackJackInProgress = false;
        //        return;
        //    }

        //    //last thing to execute

        //    server.JoinedUsers.Clear();
        //    server.BlackJackInProgress = false; 
        //    return;
        //}

        //private async Task Player1(string player1Hand1, string player1Hand2, string player, int playerHand1)
        //{
        //    embed.AddField($"{player}'s Hand", $"{player1Hand1} {player1Hand2}", true);
        //    await Task.Delay(300);
        //}

        //private static void HandValues(ref int i1, ref int i2)
        //{
        //    if (0 <= i1 && i1 <= 3) //2
        //        i1 = 2;
        //    if (0 <= i2 && i2 <= 3)
        //        i2 = 2;
        //    if (4 <= i1 && i1 <= 7) //3
        //        i1 = 3;
        //    if (4 <= i2 && i2 <= 7)
        //        i2 = 3;
        //    if (8 <= i1 && i1 <= 11) //4
        //        i1 = 4;
        //    if (8 <= i2 && i2 <= 11)
        //        i2 = 4;
        //    if (12 <= i1 && i1 <= 15) //5
        //        i1 = 5;
        //    if (12 <= i2 && i2 <= 15)
        //        i2 = 5;
        //    if (16 <= i1 && i1 <= 19) //6
        //        i1 = 6;
        //    if (16 <= i2 && i2 <= 19)
        //        i2 = 6;
        //    if (20 <= i1 && i1 <= 23) //7
        //        i1 = 7;
        //    if (20 <= i2 && i2 <= 23)
        //        i2 = 7;
        //    if (24 <= i1 && i1 <= 27) //8
        //        i1 = 8;
        //    if (24 <= i2 && i2 <= 27)
        //        i2 = 8;
        //    if (28 <= i1 && i1 <= 31) //9
        //        i1 = 9;
        //    if (28 <= i2 && i2 <= 31)
        //        i2 = 9;
        //    if (32 <= i1 && i1 <= 35) //10
        //        i1 = 10;
        //    if (32 <= i2 && i2 <= 35)
        //        i2 = 10;
        //    if (36 <= i1 && i1 <= 39) //Ace
        //        i1 = 11;
        //    if (36 <= i2 && i2 <= 39)
        //        i2 = 11;
        //    if (40 <= i1 && i1 <= 51) //Face cards
        //        i1 = 10;
        //    if (40 <= i2 && i2 <= 51)
        //        i2 = 10;
        //}

        //private static int HitCardValue(ref int i1)
        //{
        //    if (0 <= i1 && i1 <= 3) //2
        //        i1 = 2;
        //    if (4 <= i1 && i1 <= 7) //3
        //        i1 = 3;
        //    if (8 <= i1 && i1 <= 11) //4
        //        i1 = 4;
        //    if (12 <= i1 && i1 <= 15) //5
        //        i1 = 5;
        //    if (16 <= i1 && i1 <= 19) //6
        //        i1 = 6;
        //    if (20 <= i1 && i1 <= 23) //7
        //        i1 = 7;
        //    if (24 <= i1 && i1 <= 27) //8
        //        i1 = 8;
        //    if (28 <= i1 && i1 <= 31) //9
        //        i1 = 9;
        //    if (32 <= i1 && i1 <= 35) //10
        //        i1 = 10;
        //    if (36 <= i1 && i1 <= 39) //Ace
        //        i1 = 11;
        //    if (40 <= i1 && i1 <= 51) //Face cards
        //        i1 = 10;
        //    return i1;
        //}

        ////Blackjack ends

        //[Command("bjoin")]
        //public async Task JoinGame(int points)
        //{
        //    if (points < 100)
        //    {
        //        embed.WithDescription($"**{Context.User.Mention} Your bet must be at least 100 points to play!**");
        //        embed.WithColor(Red);
        //        await BE();
        //        return;
        //    }

        //    var server = Servers.GetServer(Context.Guild);
        //    var cmdPrefix = server.commandPrefix;
        //    if(server.JoinedUsers.Count >= 5 && server.BlackJackInProgress == true)
        //    {
        //        embed.WithDescription($"**{Context.User.Mention} The maximum number of players are currently playing! Please wait for their game to finish!**");
        //        embed.WithColor(Red);
        //        await BE();
        //    }
        //    else if (server.BlackJackInProgress == false)
        //    {
        //        embed.WithDescription($"**{Context.User.Mention} A game of blackjack is not currently in progress. Start one with `{cmdPrefix}gblackjack`!**");
        //        embed.WithColor(Red);
        //        await BE();
        //    }
        //    else if (server.JoinedUsers.Contains(Context.User.Username))
        //    {
        //        embed.WithDescription($"**{Context.User.Mention} You have already joined this game of blackjack!**");
        //        embed.WithColor(Red);
        //        await BE();
        //    }
        //    var playerName = Context.User.Username;
        //    var userAccount = UserAccounts.GetAccount(Context.User);
        //    uint uPoints = userAccount.Points;

        //    server.JoinedUsers.Add(Context.User.Username);

        //    embed.WithDescription($"**{Context.User.Mention} has joined the blackjack game with a bet of {points}!**");
        //    embed.WithColor(Gold);
        //    await BE();
        //}


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
