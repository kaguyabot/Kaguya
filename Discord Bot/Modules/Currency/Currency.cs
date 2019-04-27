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
using Kaguya.Core.UserAccounts;
using System.Net;
using System.Timers;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;
using Kaguya.Core;
using System.Diagnostics;



namespace Kaguya.Modules
{
    public class Currency : ModuleBase<SocketCommandContext>
    {
        public EmbedBuilder embed = new EmbedBuilder();
        public Color Pink = new Color(252, 132, 255);
        public Color Red = new Color(255, 0, 0);
        public Color Gold = new Color(255, 223, 0);
        public BotConfig bot = new BotConfig();
        public string version = Utilities.GetAlert("VERSION");
        public string botToken = Config.bot.token;
        readonly Logger logger = new Logger();
        readonly Stopwatch stopWatch = new Stopwatch();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("points")] //currency
        public async Task Points(IGuildUser user = null)
        {
            stopWatch.Start();
            if (user == null)
                user = Context.User as IGuildUser;
            var account = UserAccounts.GetAccount(user as SocketUser);
            embed.WithTitle("Points");
            embed.WithDescription($"{user.Mention} has {account.Points} points.");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("pointsadd")] //currency
        [Alias("addpoints")]
        [RequireOwner]
        public async Task PointsAdd(uint points, IGuildUser user = null)
        {
            stopWatch.Start();
            if (user == null)
            {
                var userAccount = UserAccounts.GetAccount(Context.User);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} has been awarded {points} points.");
                embed.WithColor(Pink);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
            else if (user is IGuildUser)
            {
                var userAccount = UserAccounts.GetAccount((SocketUser)user);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} has been awarded {points} points.");
                embed.WithColor(Pink);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
            }
            else
            {
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} Unable to add points to {user}! Make sure they exist and try again!");
                embed.WithColor(Pink);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User does not exist!");
            }
        }

        [Command("awardeveryone")] //currency
        [Alias("awardall")]
        [RequireOwner]
        public async Task AwardEveryone(int bonus)
        {
            stopWatch.Start();
            var userAccounts = UserAccounts.GetAllAccounts();
            int i = 1;
            foreach (UserAccount account in userAccounts)
            {
                i++;
                account.Points = (uint)(account.Points + bonus);
            }
            UserAccounts.SaveAccounts();
            embed.WithTitle("Points Awarded");
            embed.WithDescription($"{Context.User.Mention} has awarded `{bonus.ToString("N0")}` points to `{i.ToString("N0")}` users!");
            embed.WithColor(Gold);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("timelyreset")] //currency
        [RequireOwner]
        public async Task TimelyReset()
        {
            stopWatch.Start();
            var accounts = UserAccounts.GetAllAccounts();
            foreach (var account in accounts)
            {
                var difference = DateTime.Now.AddHours(-24);
                account.LastReceivedTimelyPoints = difference;
            }
            embed.WithTitle("Timely Reset");
            embed.WithDescription($"**{Context.User.Mention} Timely points for `{accounts.Count}` users have been reset!**");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("timely")] //currency
        [Alias("t")]
        public async Task DailyPoints(uint timeout = 24, double bonus = 500)
        {
            stopWatch.Start();
            Command command = Commands.GetCommand();
            var userAccount = UserAccounts.GetAccount(Context.User);
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            timeout = command.TimelyHours;
            bonus = command.TimelyPoints;
            Random rand = new Random();
            bool critical = rand.Next(100) < 14; ;
            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;


            if (!CanReceiveTimelyPoints(userAccount, (int)timeout))
            {
                var differenceTimely = DateTime.Now - userAccount.LastReceivedTimelyPoints;
                var formattedTime = $"{difference.Hours}h {difference.Minutes}m {difference.Seconds}s";
                embed.WithTitle("Timely Points");
                embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}timely`!" +
                    $" Please wait until `{timeout} hours` have passed to receive more timely points.");
                embed.WithColor(Pink);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User needs to wait before receiving more timely points.");
                return;
            }
            if (difference.TotalHours < 12) //Difference of now compared to when user last upvoted Kaguya on DBL.
                critical = rand.Next(100) < 28;
            if(critical) { bonus *= 3.50; }
            userAccount.Points += (uint)bonus;
            userAccount.LastReceivedTimelyPoints = DateTime.Now;
            UserAccounts.SaveAccounts();
            embed.WithTitle("Timely Points");
            if (critical)
                embed.WithDescription($"{Context.User.Mention} it's a critical hit! {Context.User.Username} has received `{bonus}` points! Claim again in {timeout}h.");
            else
                embed.WithDescription($"{Context.User.Mention} has received `{bonus.ToString("N0")}` points! Claim again in {timeout}h.");
            embed.WithColor(Pink);
            await BE(); stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        [Command("masspointsdistribute")] //currency
        public async Task MassPointsDistribute()
        {
            stopWatch.Start();
            UserAccount userAccount = UserAccounts.GetAccount(Context.User);
            uint userPoints = userAccount.Points;
            SocketGuild guild = Context.Guild;
            var members = Context.Guild.Users;
            int memberCount = guild.MemberCount;
            uint distributedPoints = (uint)(userPoints / memberCount);
            if (userPoints < memberCount)
            {
                embed.WithTitle("Mass Points Distribute");
                embed.WithDescription($"{Context.User.Mention} **You do not have enough points! You need at least `{memberCount - userPoints}` more points to execute this command!**");
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User does not have enough points to execute this command."); return;
            }
            if (memberCount < 25)
            {
                embed.WithTitle("Mass Points Distribute");
                embed.WithDescription($"{Context.User.Mention} **You do not have enough members in this server! 25 non-bot members must be present in the server for this command to work.**");
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "Guild does not have at least 25 non-bot members."); return;
            }
            else
            {
                foreach (var member in members)
                {
                    UserAccount memberAccount = UserAccounts.GetAccount(member);
                    memberAccount.Points += distributedPoints;
                    userAccount.Points = 0;
                }
                embed.WithTitle("Mass Points Distribute");
                embed.WithDescription($"{Context.User.Mention} **Has decided to redistribute their points balance to everyone in the server!**");
                embed.WithFooter($"{memberCount} members have been awarded {distributedPoints} points thanks to {Context.User.Username}. How generous!");
                embed.WithColor(Gold);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds); return;
            }
        }

        [Command("roll")] //currency
        [Alias("gr")]
        public async Task GamblePoints(int points)
        {
            stopWatch.Start();
            var user = Context.User;
            var userAccount = UserAccounts.GetAccount(Context.User);
            if (points > userAccount.Points)
            {
                embed.WithTitle("Gambling: Insufficient Points!");
                embed.WithDescription($"{user.Mention} you have an insufficient amount of points!" +
                    $"\nThe maximum amount you may gamble is {userAccount.Points}.");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User has insufficient points balance.");
                return;
            }
            if (points < 1)
            {
                embed.WithTitle("Gambling: Too Few Points!");
                embed.WithDescription($"{user.Mention} You may not gamble less than one point!");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User attempted to gamble less than one point.");
                return;
            }
            if (points > 25000)
            {
                embed.WithTitle("Gambling: Too Many Points!");
                embed.WithDescription($"**{user.Mention} you are attempting to gamble too many points!" +
                    $"\nThe maximum amount you may gamble is `25,000` points.**");
                embed.WithColor(Red);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User attempted to gamble more than 25,000 points.");
                return;
            }

            userAccount.Points -= (uint)points; //Takes points away from user on successful bet.

            Random rand = new Random();
            Random crit = new Random();
            var roll = rand.Next(100);
            bool critical = crit.Next(100) < 8;

            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;

            if (difference.TotalHours < 12)
                critical = crit.Next(100) < 16;

            if (roll <= 66)
            {
                userAccount.LifetimeGambleLosses++;
                userAccount.LifetimeGambles++;

                string[] sadEmotes = { "<:PepeHands:431853568669253632>", "<:FeelsBadMan:431647398071107584>", "<:FeelsWeirdMan:431148381449224192>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);
                embed.WithTitle($"Gambling: Loser! {sadEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and lost their bet of `{points.ToString("N0")}`! Better luck next time!** <:SagiriBlush:498009810692734977>");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                await BE();

                UserAccounts.SaveAccounts(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                return;
            }
            else if (67 <= roll && roll <= 78)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;

                string[] happyEmotes1 = { "<:peepoHappy:479314678699524116>", "<:EZ:431149816127553547>", "<a:pats:432262215018741780>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 2.0;
                if(critical) { multiplier *= 2.50; }
                userAccount.Points += (uint)(points * multiplier);

                if(critical)
                    embed.WithTitle($"Gambling: Winner! It's a critical hit!! {happyEmotes1[num]}");
                else
                    embed.WithTitle($"Gambling: Winner! {happyEmotes1[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                await BE();

                UserAccounts.SaveAccounts(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                return;
            }
            else if (79 <= roll && roll <= 89)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;

                string[] happyEmotes2 = { "<:Pog:484960397946912768>", "<:PogChamp:433109653501640715>", "<:nepWink:432745215217106955>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 2.50;
                if (critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                if(critical)
                    embed.WithTitle($"Gambling Winner: High Roll! It's a critical hit!! {happyEmotes2[num]}");
                else
                    embed.WithTitle($"Gambling Winner: High Roll! {happyEmotes2[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                await BE();

                UserAccounts.SaveAccounts(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                return;
            }
            else if (90 <= roll && roll <= 95)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string[] eliteEmotes = { "<:PogU:509194017368702987>", "<a:Banger:506288311829135386>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 3.75;
                if (critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                if(critical)
                    embed.WithTitle($"Gambling Winner: Elite Roll! It's a critical hit!! {eliteEmotes[num]}");
                else
                    embed.WithTitle($"Gambling Winner: Elite Roll! {eliteEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                await BE();

                UserAccounts.SaveAccounts(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                return;
            }
            else if (96 <= roll && roll <= 99)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string[] superEliteEmotes = { "<:YES:462371445864136732>", "<:smug:453259470815100941>", "<:Woww:442687161871892502>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 5.50;
                if (critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                if(critical)
                    embed.WithTitle($"Gambling Winner: Super Elite Roll! It's a critical hit!!! {superEliteEmotes[num]}");
                else
                    embed.WithTitle($"Gambling Winner: Super Elite Roll! {superEliteEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                await BE();

                UserAccounts.SaveAccounts(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                return;
            }
            else if (roll == 100)
            {

                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string sirenEmote = "<a:siren:429784681316220939>";

                var multiplier = 8.50;
                if (critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"{sirenEmote} Gambling Winner: Perfect Roll! It's a super critical hit!! {sirenEmote}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Gold); 
                await BE();

                UserAccounts.SaveAccounts(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
                return;
            }
        }

        [Command("weekly")]
        public async Task WeeklyPoints(int timeout = 168, uint bonus = 5000)
        {
            stopWatch.Start();
            UserAccount userAccount = UserAccounts.GetAccount(Context.User);
            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;
            Random crit = new Random();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            var multiplier = 3.50;
            bool critical = crit.Next(100) < 8; //8% chance of weekly being a critical roll

            if (difference.TotalHours < 12)
                critical = crit.Next(100) < 16; //16% if they've upvoted Kaguya within the last 12 hours.

            if(!CanReceiveWeeklyPoints(userAccount, timeout))
            {
                var weeklyDifference = DateTime.Now - userAccount.LastReceivedWeeklyPoints;
                var formattedTime = $"{weeklyDifference.Days}d {weeklyDifference.Hours}h {weeklyDifference.Minutes}m {weeklyDifference.Seconds}s";
                embed.WithTitle("Weekly Points");
                embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}weekly`!" +
                    $" Please wait until `7 days` have passed to receive your weekly bonus.");
                embed.WithColor(Pink);
                await BE(); stopWatch.Stop();
                logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User has not waited for the weekly bonus timer to reset.");
                return;
            }

            userAccount.LastReceivedWeeklyPoints = DateTime.Now;

            if(critical)
            {
                bonus = (uint)(bonus * multiplier);
                embed.WithDescription($"**{Context.User.Mention} has received their weekly bonus of `{bonus}` points! It's a critical hit!!**");
            }
            else
                embed.WithDescription($"**{Context.User.Mention} has received their weekly bonus of `{bonus}` points!**");
            embed.WithColor(Pink);
            await BE();

            UserAccounts.SaveAccounts();
            stopWatch.Stop();
            logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds);
        }

        internal static bool CanReceiveTimelyPoints(UserAccount user, int timeout)
        {
            var difference = DateTime.Now - user.LastReceivedTimelyPoints;
            return difference.TotalHours > timeout;
        }

        internal static bool CanReceiveWeeklyPoints(UserAccount user, int timeout)
        {
            var difference = DateTime.Now - user.LastReceivedWeeklyPoints;
            return difference.TotalHours > timeout;
        }

    }
}
