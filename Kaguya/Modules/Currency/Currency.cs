using System;
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
using EmbedColor = Kaguya.Core.Embed.EmbedColor;
using Kaguya.Core;
using System.Collections.Generic;
using Discord.Addons.Interactive;

namespace Kaguya.Modules
{
    public class Currency : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        private readonly Logger logger = new Logger();

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
            embed.WithDescription($"{user.Mention} has `{account.Points.ToString("N0")}` points.");
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
            bool critical = rand.Next(101) < 14;
            var difference = DateTime.Now - userAccount.LastReceivedTimelyPoints;

            if (!CanReceiveTimelyPoints(userAccount, (int)timeout))
            {
                var formattedTime = $"{difference.Hours}h {difference.Minutes}m {difference.Seconds}s";
                embed.WithTitle("Timely Points");
                embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}timely`!" +
                    $" Please wait until `{timeout} hours` have passed to receive more timely points.");
                await BE();
                logger.ConsoleStatusAdvisory("User is unable to receive timely points at this time.");
                return;
            }

            if (difference.TotalHours < 12 || userAccount.IsSupporter) //Difference of now compared to when user last upvoted Kaguya on DBL.
                critical = rand.Next(101) < 22;

            if (difference.TotalHours < 12 && userAccount.IsSupporter)
                critical = rand.Next(101) < 30;

            if(critical) { bonus *= 3.50; }

            userAccount.Points += (uint)bonus;
            userAccount.TotalCurrencyAwarded += (int)bonus;
            userAccount.LastReceivedTimelyPoints = DateTime.Now;

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
            var diamonds = userAccount.Diamonds;
            stopWatch.Stop();

            await GlobalCommandResponses.CreateCommandResponse(Context,
                description: $"{Context.User.Mention} You currently have <a:KaguyaDiamonds:581562698228301876> **`{diamonds.ToString("N0")}`.**",
                footer: $"Diamonds are given to Kaguya Supporters. Find out more with {cmdPrefix}supporter.");
        }

        [Command("diamondconvert")] //Currency
        [Alias("dc")]
        public async Task DiamondConvert(int diamonds)
        {
            var userAccount = UserAccounts.GetAccount(Context.User);

            if (diamonds > 0)
            {
                if (userAccount.Diamonds < diamonds)
                {
                    embed.WithDescription($"{Context.User.Mention} You don't have enough <a:KaguyaDiamonds:581562698228301876>");
                    embed.SetColor(EmbedColor.RED);
                    await BE(); return;
                }

                userAccount.Points += (uint)(diamonds * 100);
                userAccount.TotalCurrencyAwarded += (diamonds * 100);
                userAccount.Diamonds -= (uint)diamonds;

                embed.WithDescription($"{Context.User.Mention} **Successfully converted " +
                    $"<a:KaguyaDiamonds:581562698228301876>`{diamonds.ToString("N0")}` into `{(diamonds * 100).ToString("N0")}` points.**");
                embed.WithFooter($"New Totals - Diamonds: {userAccount.Diamonds.ToString("N0")} Points: {userAccount.Points.ToString("N0")}");
                await BE();
                logger.ConsoleStatusAdvisory($"User successfully converted {diamonds} into points.");
            }
            else
            {
                embed.WithDescription($"{Context.User.Mention} A minimum of <a:KaguyaDiamonds:581562698228301876>`1` is required!");
                embed.SetColor(EmbedColor.RED);
                await BE(); return;
            }

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
                }
                userAccount.Points = 0;
                userAccount.TotalCurrencyLost += (int)distributedPoints;

                embed.WithTitle("Mass Points Distribute");
                embed.WithDescription($"{Context.User.Mention} **Has decided to redistribute their points balance to everyone in the server!**");
                embed.WithFooter($"{memberCount} members have been awarded {distributedPoints} points thanks to {Context.User.Username}. How generous!");
                embed.SetColor(EmbedColor.GOLD);

                logger.ConsoleStatusAdvisory("User successfully redistributed all of their points.");

                await BE();
            }
        }

        private void GambleHistory(UserAccount uAccount, int roll, 
            int pointsGambled, int pointsWonLost, int luck, bool winner = true)
        {
            Logger logger = new Logger();

            if (uAccount.GambleHistory.Count >= 10)
                uAccount.GambleHistory.RemoveAt(0);

            int j = 0;

            if (luck >= 5)
            {
                for (int i = 0; i < luck; i++)
                {
                    j += 2;
                }
            }

            if (winner)
            {
                logger.ConsoleInformationAdvisory($"Gambling: User {uAccount.Username} - Roll: {roll} - Points Gambled: {pointsGambled} - Points Won: {pointsWonLost} - Luck: {j}");
                uAccount.GambleHistory.Add($"\n🔵 `Roll:` `{roll}` - Points Gambled: `{pointsGambled.ToString("N0")}` - " +
                    $"Points Won: `{pointsWonLost.ToString("N0")}` - `{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}`");
            }
            else
            {
                logger.ConsoleInformationAdvisory($"Gambling: User {uAccount.Username} - Roll: {roll} - Points Lost: {pointsWonLost} - Luck: {j}");
                uAccount.GambleHistory.Add($"\n🔴 `Roll:` `{roll}` - Points Gambled: `{pointsGambled.ToString("N0")}` - " +
                    $"Points Lost: `{pointsWonLost.ToString("N0")}` - `{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}`");
            }
        }

        private void QuickdrawHistory(UserAccount uAccount, double KRoll, 
            double URoll, int pointsGambled, int reward, bool winner,
            bool critical)
        {
            Logger logger = new Logger();

            if (uAccount.GambleHistory.Count >= 10)
                uAccount.GambleHistory.RemoveAt(0);

            if(winner)
            {
                logger.ConsoleInformationAdvisory($"Quickdraw: Winner - User {uAccount.ID} | {uAccount.Username}" +
                    $" - Points Gambled: {pointsGambled.ToString("N0")} - Points Won: {reward.ToString("N0")}" +
                    $" - Kaguya Time: {KRoll.ToString("N3")}s - User Time: {URoll.ToString("N3")}s - Critical: {critical}");
                uAccount.GambleHistory.Add($"\n🔵 `QD:` `KTime: {KRoll.ToString("N3")}s` - `UTime: {URoll.ToString("N3")}s` - " +
                    $"Pts: `{pointsGambled.ToString("N0")}` - Pts Awarded: `{reward.ToString("N0")}` - " +
                    $"`{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}`");
            }
            else if(!winner)
            {
                logger.ConsoleInformationAdvisory($"Quickdraw: Loser - User {uAccount.ID} | {uAccfount.Username}" +
                    $" - Points Lost: {pointsGambled.ToString("N0")}" +
                    $" - Kaguya Time: {KRoll.ToString("N3")}s - User Time: {URoll.ToString("N3")}s");
                uAccount.GambleHistory.Add($"\n🔴 `QD:` `KTime: {KRoll.ToString("N3")}s` - `UTime: {URoll.ToString("N3")}s` - " +
                    $"Pts: `{pointsGambled.ToString("N0")}` - " +
                    $"`{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}`");
            }
        }

        [Command("roll")]
        [Alias("gr")]
        public async Task GamblePoints(int points)
        {
            var user = Context.User;
            var userAccount = UserAccounts.GetAccount(Context.User);
            Logger logger = new Logger();

            if (points > userAccount.Points)
            {
                embed.WithTitle("Gambling: Insufficient Points!");
                embed.WithDescription($"{user.Mention} you have an insufficient amount of points!" +
                    $"\nThe maximum amount you may gamble is {userAccount.Points}.");
                embed.SetColor(EmbedColor.RED);
                await BE();
                logger.ConsoleInformationAdvisory($"User does not have enough points to gamble the requested amount.");
                return;
            }
            if (points < 1)
            {
                embed.WithTitle("Gambling: Too Few Points!");
                embed.WithDescription($"{user.Mention} You may not gamble less than one point!");
                embed.SetColor(EmbedColor.RED);
                await BE();
                logger.ConsoleInformationAdvisory($"User may not gamble less than one point.");
                return;
            }
            if (points > 25000 && !userAccount.IsSupporter)
            {
                embed.WithTitle("Gambling: Too Many Points!");
                embed.WithDescription($"**{user.Mention} you are attempting to gamble too many points!" +
                    $"\nThe maximum amount you may gamble is `25,000` points.**");
                embed.SetColor(EmbedColor.RED);
                await BE();
                logger.ConsoleInformationAdvisory("User attempted to gamble too many points.");
                return;
            }
            if (points > 500000 && userAccount.IsSupporter)
            {
                embed.WithDescription($"{Context.User.Mention} You are attempting to bet too many points " +
                    $"(must be less than 500,000).");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            userAccount.Points -= (uint)points; //Takes points away from user on successful bet.

            Random rand = new Random();
            Random crit = new Random();
            var roll = rand.Next(101);
            bool critical = crit.Next(101) < 5;

            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;

            if (difference.TotalHours < 12)
                critical = crit.Next(101) < 10;

            if (userAccount.IsSupporter)
                critical = crit.Next(101) < 10;

            if (userAccount.IsSupporter && difference.TotalHours < 12)
                critical = crit.Next(101) < 20;

            if (userAccount.GamblingBadLuckStreak >= 5 && roll < 67)
            {
                for(int i = 0; i < userAccount.GamblingBadLuckStreak; i++)
                {
                    roll += 2;
                    if (roll > 100)
                        roll = 100;
                }
            }

            if ((userAccount.LifetimeGambleWins / userAccount.LifetimeGambles) > 45 && userAccount.LifetimeGambles > 15)
                roll -= 5;

            if (roll <= 66)
            {
                userAccount.LifetimeGambleLosses++;
                userAccount.LifetimeGambles++;
                userAccount.GamblingBadLuckStreak++;
                userAccount.TotalCurrencyLost += points;
                userAccount.TotalCurrencyGambled += points;
                GambleHistory(userAccount, roll, points, points, userAccount.GamblingBadLuckStreak, false);

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
                userAccount.TotalCurrencyGambled += points;

                int luck = userAccount.GamblingBadLuckStreak;
                userAccount.GamblingBadLuckStreak = 0;

                string[] happyEmotes1 = { "<:peepoHappy:479314678699524116>", "<:EZ:431149816127553547>", "<a:pats:432262215018741780>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 1.70;
                if(critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                userAccount.TotalCurrencyAwarded += (int)(points * multiplier);
                GambleHistory(userAccount, roll, points, (int)(points * multiplier), luck);

                if (critical)
                    embed.WithTitle($"Gambling: Winner! It's a critical hit!! {happyEmotes1[num]}");
                else
                {
                    embed.WithTitle($"Gambling: Winner! {happyEmotes1[num]}");
                    userAccount.TotalCurrencyAwarded += points;
                }

                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                await BE();
            }
            else if (79 <= roll && roll <= 89)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.TotalCurrencyGambled += points;

                int luck = userAccount.GamblingBadLuckStreak;
                userAccount.GamblingBadLuckStreak = 0;

                string[] happyEmotes2 = { "<:Pog:484960397946912768>", "<:PogChamp:433109653501640715>", "<:nepWink:432745215217106955>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 2.50;
                if (critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                userAccount.TotalCurrencyAwarded += (int)(points * multiplier);
                GambleHistory(userAccount, roll, points, (int)(points * multiplier), luck);

                if (critical)
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
                userAccount.TotalCurrencyGambled += points;

                int luck = userAccount.GamblingBadLuckStreak;
                userAccount.GamblingBadLuckStreak = 0;


                string[] eliteEmotes = { "<:PogU:509194017368702987>", "<a:Banger:506288311829135386>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 3.00;
                if (critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                userAccount.TotalCurrencyAwarded += (int)(points * multiplier);
                GambleHistory(userAccount, roll, points, (int)(points * multiplier), luck);

                if (critical)
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
                userAccount.TotalCurrencyGambled += points;

                int luck = userAccount.GamblingBadLuckStreak;
                userAccount.GamblingBadLuckStreak = 0;

                string[] superEliteEmotes = { "<:YES:462371445864136732>", "<:smug:453259470815100941>", "<:Woww:442687161871892502>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 4.25;
                if (critical) { multiplier *= 2.50; }

                userAccount.Points += (uint)(points * multiplier);
                userAccount.TotalCurrencyAwarded += (int)(points * multiplier);
                GambleHistory(userAccount, roll, points, (int)(points * multiplier), luck);

                if (critical)
                    embed.WithTitle($"Gambling Winner: Super Elite Roll! It's a critical hit!!! {superEliteEmotes[num]}");
                else
                    embed.WithTitle($"Gambling Winner: Super Elite Roll! {superEliteEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                await BE();
            }
            else if (roll == 100)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;
                userAccount.TotalCurrencyGambled += points;

                int luck = userAccount.GamblingBadLuckStreak;
                userAccount.GamblingBadLuckStreak = 0;

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
                userAccount.TotalCurrencyAwarded += (int)(points * multiplier);
                GambleHistory(userAccount, roll, points, (int)(points * multiplier), luck);

                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.SetColor(EmbedColor.GOLD);
                await BE();
            }
        }

        [Command("history")]
        [Alias("gh")]
        public async Task GambleHistory()
        {
            UserAccount userAccount = UserAccounts.GetAccount(Context.User);
            string history = "";

            foreach (var item in userAccount.GambleHistory)
                history += item;

            embed.AddField($"Kaguya Gambling History for {Context.User.Username}", history);
            await BE();
        }
        

        [Command("weekly")]
        public async Task WeeklyPoints(int timeout = 168, uint bonus = 5000)
        {
            UserAccount userAccount = UserAccounts.GetAccount(Context.User);
            Logger logger = new Logger();
            var difference = DateTime.Now - userAccount.LastUpvotedKaguya;
            Random crit = new Random();
            var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;
            var multiplier = 3.50;
            bool critical = crit.Next(100) < 8; //8% chance of weekly being a critical hit

            if (difference.TotalHours < 12 || userAccount.IsSupporter)
                critical = crit.Next(100) < 12; //12% if they've upvoted Kaguya within the last 12 hours or they are a supporter.

            if (difference.TotalHours < 12 && userAccount.IsSupporter) //24% critical chance if upvoted and supporter.
                critical = crit.Next(100) < 24;

            if (!CanReceiveWeeklyPoints(userAccount, timeout))
            {
                var weeklyDifference = DateTime.Now - userAccount.LastReceivedWeeklyPoints;
                var formattedTime = $"{weeklyDifference.Days}d {weeklyDifference.Hours}h {weeklyDifference.Minutes}m {weeklyDifference.Seconds}s";
                embed.WithTitle("Weekly Points");
                embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}weekly`!" +
                    $" Please wait until `7 days` have passed to receive your weekly bonus.");
                embed.SetColor(EmbedColor.RED);
                await BE();
                logger.ConsoleInformationAdvisory($"User {Context.User} has not waited for their weekly points cooldown to reset.");
                return;
            }

            userAccount.LastReceivedWeeklyPoints = DateTime.Now;

            if (critical)
            {
                bonus = (uint)(bonus * multiplier);
                embed.WithDescription($"**{Context.User.Mention} has received their weekly bonus of `{bonus.ToString("N0")}` points! It's a critical hit!!**");
                logger.ConsoleInformationAdvisory("User successfully claimed weekly points. Critical hit.");
            }
            else
            {
                embed.WithDescription($"**{Context.User.Mention} has received their weekly bonus of `{bonus.ToString("N0")}` points!**");
                logger.ConsoleStatusAdvisory("User successfully claimed weekly points. Non critical.");
            }
            await BE();
            userAccount.Points += bonus;
        }

        [Command("quickdraw")]
        [Alias("qd")]
        public async Task CurrencyRaid(int points)
        {
            var userAccount = UserAccounts.GetAccount(Context.User);

            if(points > 25000 && !userAccount.IsSupporter)
            {
                embed.WithDescription($"{Context.User.Mention} You may not bet more than 25,000 points.");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            if(points > 500000 && userAccount.IsSupporter)
            {
                embed.WithDescription($"{Context.User.Mention} You are attempting to bet too many points " +
                    $"(must be less than 500,000).");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            if (points < 50)
            {
                embed.WithDescription($"{Context.User.Mention} You may not bet less than 50 points.");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            if(userAccount.Points < points)
            {
                embed.WithDescription($"You are attempting to bet too many points.");
                embed.SetColor(EmbedColor.RED);
                await BE();
                return;
            }

            userAccount.TotalCurrencyGambled += points;

            Random rand = new Random();
            double kaguyaDraw = rand.NextDouble();
            double userDraw = rand.NextDouble();

            bool critical = rand.Next(101) < 2; //2% Critical chance.
            if (userAccount.IsSupporter || userAccount.IsBenefitingFromUpvote)
                critical = rand.Next(101) < 4; //4% chance if supporter or has recently upvoted.
            if (userAccount.IsSupporter && userAccount.IsBenefitingFromUpvote)
                critical = rand.Next(101) < 6; //6% chance if supporter + has recently upvoted.
            double multiplier = 1.75;

            userAccount.TotalCurrencyGambled += points;

            if (userDraw > kaguyaDraw)
            {
                userAccount.Points -= (uint)points;
                userAccount.TotalCurrencyLost += points;

                embed.WithDescription($"🔫 **You lost!** - {Context.User.Mention} " +
                    $"has lost `{points.ToString("N0")}` points" +
                    $"\n" +
                    $"\nKaguya's Time: `{kaguyaDraw.ToString("N3")}` seconds" +
                    $"\n{Context.User.Username}'s Time: `{userDraw.ToString("N3")}` seconds");
                embed.WithFooter($"Better luck next time! - Points Balance: {userAccount.Points.ToString("N0")}");
                embed.SetColor(EmbedColor.RED);
                await BE();

                QuickdrawHistory(userAccount, kaguyaDraw, userDraw, points, 0, false, critical);
            }

            if (kaguyaDraw > userDraw)
            {
                string critText = "";
                if (critical)
                {
                    multiplier = 2.40; //Puts total bonus at around 2.89x what they bet
                    critText += " It's a critical hit!!";
                }

                int award = (int)(points * multiplier);
                userAccount.Points += (uint)award;
                userAccount.TotalCurrencyAwarded += award;

                embed.WithDescription($"🔫 **You won!{critText}** - {Context.User.Mention} " +
                    $"has won `{award.ToString("N0")}` points" +
                    $"\n" +
                    $"\nKaguya's Time: `{kaguyaDraw.ToString("N3")}` seconds" + 
                    $"\n{Context.User.Username}'s Time: `{userDraw.ToString("N3")}` seconds");
                embed.WithFooter($"Aced! - Points Balance: {userAccount.Points.ToString("N0")}");
                embed.SetColor(EmbedColor.GREEN);
                await BE();

                QuickdrawHistory(userAccount, kaguyaDraw, userDraw, points, award, true, critical);
            }

            if(userDraw == kaguyaDraw)
            {
                embed.WithDescription($"🔫 **It's a draw!!** - {Context.User.Mention}" +
                    $"\n" +
                    $"\nKaguya's Time: `{kaguyaDraw.ToString("N3")}` seconds" +
                    $"\n{Context.User.Username}'s Time: `{userDraw.ToString("N3")}` seconds");
                embed.WithFooter($"What are the chances?! - Points Balance: {userAccount.Points.ToString("N0")}");
                embed.SetColor(EmbedColor.GOLD);
                await BE();
                logger.ConsoleStatusAdvisory($"User {Context.User} - Quickdraw: Tie");
            }
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
