﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.UserAccounts;
using Kaguya.Core.Server_Files;
using Kaguya.Core.Commands;
using System.Diagnostics;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.Embed;
using EmbedType = Kaguya.Core.Embed.EmbedType;

namespace Kaguya.Modules
{
    public class Currency : ModuleBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("points")] //currency
        public async Task Points(IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;
            var account = UserAccounts.GetAccount(user as SocketUser);
            embed.WithTitle("Points");
            embed.WithDescription($"{user.Mention} has {account.Points} points.");
            await BE();
        }

        [Command("pointsadd")] //currency
        [Alias("addpoints")]
        [RequireOwner]
        public async Task PointsAdd(uint points, IGuildUser user = null)
        {
            if (user == null)
            {
                var userAccount = UserAccounts.GetAccount(Context.User);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{user.Mention} has been awarded {points} points.");
                await BE();
            }
            else if (user is IGuildUser)
            {
                var userAccount = UserAccounts.GetAccount((SocketUser)user);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{user.Mention} has been awarded {points} points.");
                await BE();
            }
            else
            {
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} Unable to add points to {user}! Make sure they exist and try again!");
                await BE();
            }
        }

        [Command("bugaward")]
        [RequireOwner]
        public async Task BugAward(IUser user)
        {
            var userAccount = UserAccounts.GetAccount(user as SocketUser);
            userAccount.Points += 2000;
            UserAccounts.SaveAccounts();

            await user.GetOrCreateDMChannelAsync();
            await user.SendMessageAsync($"Hello, you reported a bug that led to a fix! As a reward, `2,000 Kaguya Points` have been added to your account. Thank you for your contribution, and " +
                $"if you notice anymore bugs, don't hesitate to keep using the bug reporter!" +
                $"\n" +
                $"\n-Stage.");

            embed.WithDescription($"{Context.User.Mention} The user has been rewarded and was sent a DM.");
            await BE();
        }

        [Command("awardeveryone")] //currency
        [Alias("awardall")]
        [RequireOwner]
        public async Task AwardEveryone(int bonus)
        {
            var userAccounts = UserAccounts.GetAllAccounts();
            int i = 0;
            foreach (UserAccount account in userAccounts)
            {
                i++;
                account.Points = (uint)(account.Points + bonus);
            }
            UserAccounts.SaveAccounts();
            embed.WithTitle("Points Awarded");
            embed.WithDescription($"{Context.User.Mention} has awarded `{bonus.ToString("N0")}` points to `{i.ToString("N0")}` users!");
            embed.SetColor(EmbedType.GOLD);
            await BE();
        }

        [Command("timelyreset")] //currency
        [RequireOwner]
        public async Task TimelyReset()
        {
            var accounts = UserAccounts.GetAllAccounts();
            foreach (var account in accounts)
            {
                var difference = DateTime.Now.AddHours(-24);
                account.LastReceivedTimelyPoints = difference;
            }
            embed.WithTitle("Timely Reset");
            embed.WithDescription($"**{Context.User.Mention} Timely points for `{accounts.Count}` users have been reset!**");
            
            await BE();
        }

        [Command("timely")] //currency
        [Alias("t")]
        public async Task DailyPoints(uint timeout = 24, double bonus = 500)
        {
            Command command = Commands.GetCommand();
            var userAccount = UserAccounts.GetAccount(Context.User);
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

            timeout = command.TimelyHours;
            bonus = command.TimelyPoints;
            Random rand = new Random();
            bool critical = rand.Next(100) < 14;
            var difference = DateTime.Now - userAccount.LastReceivedTimelyPoints;
            var supporterTime = userAccount.KaguyaSupporterExpiration - DateTime.Now;

            if (!CanReceiveTimelyPoints(userAccount, (int)timeout))
            {
                var formattedTime = $"{difference.Hours}h {difference.Minutes}m {difference.Seconds}s";
                embed.WithTitle("Timely Points");
                embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}timely`!" +
                    $" Please wait until `{timeout} hours` have passed to receive more timely points.");
                await BE();
                return;
            }
            if (difference.TotalHours < 12) //Difference of now compared to when user last upvoted Kaguya on DBL.
                critical = rand.Next(100) < 28;
            if (supporterTime.TotalSeconds > 0)
                critical = rand.Next(100) < 28;
            if (difference.TotalHours < 12 && supporterTime.TotalSeconds > 0)
                critical = rand.Next(100) < 56;
            if(critical) { bonus *= 3.50; }
            userAccount.Points += (uint)bonus;
            userAccount.LastReceivedTimelyPoints = DateTime.Now;
            UserAccounts.SaveAccounts();
            embed.WithTitle("Timely Points");
            if (critical)
                embed.WithDescription($"{Context.User.Mention} it's a critical hit! {Context.User.Username} has received `{bonus}` points! Claim again in {timeout}h.");
            else
                embed.WithDescription($"{Context.User.Mention} has received `{bonus.ToString("N0")}` points! Claim again in {timeout}h.");
            await BE();
        }

        [Command("diamonds")]
        public async Task Diamonds()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            string cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            var userAccount = UserAccounts.GetAccount(Context.User);
            var diamonds = userAccount.KaguyaDiamonds;
            stopWatch.Stop();

            await GlobalCommandResponses.CreateCommandResponse(Context,
                stopWatch.ElapsedMilliseconds,
                description: $"{Context.User.Mention} You currently have <a:KaguyaDiamonds:581562698228301876> **`{diamonds.ToString("N0")}`.**",
                footer: $"Diamonds are given to Kaguya Supporters. Find out more with {cmdPrefix}supporter.");
        }

        [Command("masspointsdistribute")] //currency
        public async Task MassPointsDistribute()
        {
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
                await BE();
            }
            if (memberCount < 25)
            {
                embed.WithTitle("Mass Points Distribute");
                embed.WithDescription($"{Context.User.Mention} **You do not have enough members in this server! 25 non-bot members must be present in the server for this command to work.**");
                await BE();
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
                embed.SetColor(EmbedType.GOLD);
                await BE();
            }
        }

        [Command("roll")] //currency
        [Alias("gr")]
        public async Task GamblePoints(int points)
        {
            var user = Context.User;
            var userAccount = UserAccounts.GetAccount(Context.User);
            if (points > userAccount.Points)
            {
                embed.WithTitle("Gambling: Insufficient Points!");
                embed.WithDescription($"{user.Mention} you have an insufficient amount of points!" +
                    $"\nThe maximum amount you may gamble is {userAccount.Points}.");
                await BE();
                return;
            }
            if (points < 1)
            {
                embed.WithTitle("Gambling: Too Few Points!");
                embed.WithDescription($"{user.Mention} You may not gamble less than one point!");
                await BE();
                return;
            }
            if (points > 25000 && !((userAccount.KaguyaSupporterExpiration - DateTime.Now).TotalSeconds > 0))
            {
                embed.WithTitle("Gambling: Too Many Points!");
                embed.WithDescription($"**{user.Mention} you are attempting to gamble too many points!" +
                    $"\nThe maximum amount you may gamble is `25,000` points.**");
                await BE();
                return;
            }

            userAccount.Points -= (uint)points; //Takes points away from user on successful bet.

            Random rand = new Random();
            Random crit = new Random();
            var roll = rand.Next(101);
            bool critical = crit.Next(100) < 8;

            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;
            var supporterTime = userAccount.KaguyaSupporterExpiration - DateTime.Now;

            if (difference.TotalHours < 12)
                critical = crit.Next(100) < 16;

            if (supporterTime.TotalSeconds > 0)
                critical = crit.Next(100) < 16;

            if (supporterTime.TotalSeconds > 0 && difference.TotalHours < 12)
                critical = crit.Next(100) < 32;

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
                await BE();
            }
            else if (67 <= roll && roll <= 78)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;

                string[] happyEmotes1 = { "<:peepoHappy:479314678699524116>", "<:EZ:431149816127553547>", "<a:pats:432262215018741780>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 1.70;
                if(critical) { multiplier *= 2.50; }
                userAccount.Points += (uint)(points * multiplier);

                if(critical)
                    embed.WithTitle($"Gambling: Winner! It's a critical hit!! {happyEmotes1[num]}");
                else
                    embed.WithTitle($"Gambling: Winner! {happyEmotes1[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                await BE();
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
                await BE();
            }
            else if (90 <= roll && roll <= 95)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string[] eliteEmotes = { "<:PogU:509194017368702987>", "<a:Banger:506288311829135386>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 3.00;
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
                await BE();
            }
            else if (96 <= roll && roll <= 99)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string[] superEliteEmotes = { "<:YES:462371445864136732>", "<:smug:453259470815100941>", "<:Woww:442687161871892502>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 4.25;
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
                await BE();

                UserAccounts.SaveAccounts();
            }
            else if (roll == 100)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string sirenEmote = "<a:siren:429784681316220939>";

                var multiplier = 6.00;
                if (critical)
                {
                    multiplier *= 5.00;
                    embed.WithTitle($"{sirenEmote} Gambling Winner: Perfect Roll! It's a super critical hit!! {sirenEmote}");
                }
                else
                    embed.WithTitle($"{sirenEmote} Gambling Winner: Perfect Roll! {sirenEmote}");

                userAccount.Points += (uint)(points * multiplier);
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.SetColor(EmbedType.GOLD);
                await BE();
            }
        }

        [Command("weekly")]
        public async Task WeeklyPoints(int timeout = 168, uint bonus = 5000)
        {
            UserAccount userAccount = UserAccounts.GetAccount(Context.User);
            var supporterTime = userAccount.KaguyaSupporterExpiration - DateTime.Now;
            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;
            Random crit = new Random();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            var multiplier = 3.50;
            bool critical = crit.Next(100) < 8; //8% chance of weekly being a critical roll

            if (difference.TotalHours < 12)
                critical = crit.Next(100) < 16; //16% if they've upvoted Kaguya within the last 12 hours.

            if (supporterTime.TotalSeconds > 0)
                critical = crit.Next(100) < 16;

            if (supporterTime.TotalSeconds > 0 && difference.TotalHours < 12)
                critical = crit.Next(100) < 32;

            if (!CanReceiveWeeklyPoints(userAccount, timeout))
            {
                var weeklyDifference = DateTime.Now - userAccount.LastReceivedWeeklyPoints;
                var formattedTime = $"{weeklyDifference.Days}d {weeklyDifference.Hours}h {weeklyDifference.Minutes}m {weeklyDifference.Seconds}s";
                embed.WithTitle("Weekly Points");
                embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}weekly`!" +
                    $" Please wait until `7 days` have passed to receive your weekly bonus.");
                embed.SetColor(EmbedType.RED);
                await BE();
                // logger.ConsoleCommandLog(Context, stopWatch.ElapsedMilliseconds, CommandError.Unsuccessful, "User has not waited for the weekly bonus timer to reset."); CREATE ERROR HANDLER
                return;
            }

            userAccount.LastReceivedWeeklyPoints = DateTime.Now;

            if (critical)
            {
                bonus = (uint)(bonus * multiplier);
                embed.WithDescription($"**{Context.User.Mention} has received their weekly bonus of `{bonus}` points! It's a critical hit!!**");
            }
            else
            {
                embed.WithDescription($"**{Context.User.Mention} has received their weekly bonus of `{bonus}` points!**");
            }
            await BE();

            userAccount.Points += bonus;
            UserAccounts.SaveAccounts();
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
