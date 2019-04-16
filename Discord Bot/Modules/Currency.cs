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

#pragma warning disable

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

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("points")] //currency
        public async Task Points(IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;
            var account = UserAccounts.GetAccount(user as SocketUser);
            embed.WithTitle("Points");
            embed.WithDescription($"{user.Mention} has {account.Points} points.");
            embed.WithColor(Pink);
            BE();
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
                embed.WithDescription($"{Context.User.Mention} has been awarded {points} points.");
                embed.WithColor(Pink);
                BE();
            }
            else if (user is IGuildUser)
            {
                var userAccount = UserAccounts.GetAccount((SocketUser)user);
                userAccount.Points += points;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} has been awarded {points} points.");
                embed.WithColor(Pink);
                BE();
            }
            else
            {
                embed.WithTitle("Adding Points");
                embed.WithDescription($"{Context.User.Mention} Unable to add points to {user}! Make sure they exist and try again!");
                embed.WithColor(Pink);
                BE();
            }
        }

        [Command("awardeveryone")] //currency
        [Alias("awardall")]
        [RequireOwner]
        public async Task AwardEveryone(int bonus)
        {
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
            BE();
        }

        [Command("timelyreset")] //currency
        [RequireOwner]
        public async Task TimelyReset()
        {
            var commands = Commands.GetCommand();
            var accounts = UserAccounts.GetAllAccounts();
            foreach (var account in accounts)
            {
                var difference = DateTime.Now.AddHours(-24);
                account.LastReceivedTimelyPoints = difference;
            }
            embed.WithTitle("Timely Reset");
            embed.WithDescription($"**{Context.User.Mention} Timely points for `{accounts.Count}` users have been reset!**");
            embed.WithColor(Pink);
            BE();
        }

        [Command("timely")] //currency
        [Alias("t")]
        public async Task DailyPoints(uint timeout = 24, uint bonus = 500)
        {
            try
            {
                Command command = Commands.GetCommand();

                timeout = command.TimelyHours;
                bonus = command.TimelyPoints;

                var cmdPrefix = Servers.GetServer(Context.Guild).commandPrefix;

                var userAccount = UserAccounts.GetAccount(Context.User);
                if (!CanReceiveTimelyPoints(userAccount, (int)timeout))
                {
                    var difference = DateTime.Now - userAccount.LastReceivedTimelyPoints;
                    var formattedTime = $"{difference.Hours}h {difference.Minutes}m {difference.Seconds}s";
                    embed.WithTitle("Timely Points");
                    embed.WithDescription($"{Context.User.Mention} It's only been `{formattedTime}` since you've used `{cmdPrefix}timely`!" +
                        $" Please wait until `{timeout} hours` have passed to receive more timely points.");
                    embed.WithColor(Pink);
                    BE();
                    return;
                }
                userAccount.Points += bonus;
                userAccount.LastReceivedTimelyPoints = DateTime.Now;
                UserAccounts.SaveAccounts();
                embed.WithTitle("Timely Points");
                embed.WithDescription($"{Context.User.Mention} has received {bonus} points! Claim again in {timeout}h.");
                embed.WithColor(Pink);
                BE();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception " + e.InnerException);
                Console.WriteLine("Message " + e.Message);
                Console.WriteLine("Stack Trace " + e.StackTrace);
            }
        }

        internal static bool CanReceiveTimelyPoints(UserAccount user, int timeout)
        {
            var difference = DateTime.Now - user.LastReceivedTimelyPoints;
            return difference.TotalHours > timeout;
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
                BE(); return;
            }
            if (memberCount < 25)
            {
                embed.WithTitle("Mass Points Distribute");
                embed.WithDescription($"{Context.User.Mention} **You do not have enough members in this server! 25 non-bot members must be present in the server for this command to work.**");
                BE(); return;
            }
            else
            {
                foreach (var member in members)
                {
                    UserAccount memberAccount = UserAccounts.GetAccount(member);
                    memberAccount.Points = memberAccount.Points + distributedPoints;
                    userAccount.Points = 0;
                }
                embed.WithTitle("Mass Points Distribute");
                embed.WithDescription($"{Context.User.Mention} **Has decided to redistribute their points balance to everyone in the server!**");
                embed.WithFooter($"{memberCount} members have been awarded {distributedPoints} points thanks to {Context.User.Username}. How generous!");
                embed.WithColor(Gold);
                BE(); return;
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
                embed.WithColor(Red);
                BE();
                return;
            }
            if (points > 25000)
            {
                embed.WithTitle("Gambling: Too Many Points!");
                embed.WithDescription($"**{user.Mention} you are attempting to gamble too many points!" +
                    $"\nThe maximum amount you may gamble is `25,000` points.**");
                embed.WithColor(Red);
                BE();
                return;
            }

            userAccount.Points -= (uint)points; //Takes points away from user on successful bet.

            Random rand = new Random();
            var roll = rand.Next(0, 100);

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
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
            else if (67 <= roll && roll <= 78)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;

                string[] happyEmotes1 = { "<:peepoHappy:479314678699524116>", "<:EZ:431149816127553547>", "<a:pats:432262215018741780>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 1.25;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling: Winner! {happyEmotes1[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
            else if (79 <= roll && roll <= 89)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;

                string[] happyEmotes2 = { "<:Pog:484960397946912768>", "<:PogChamp:433109653501640715>", "<:nepWink:432745215217106955>" };
                Random randEmote = new Random();
                var num = randEmote.Next(0, 2);

                var multiplier = 1.75;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling Winner: High Roll! {happyEmotes2[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
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

                var multiplier = 2.25;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling Winner: Elite Roll! {eliteEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
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

                var multiplier = 3.00;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"Gambling Winner: Super Elite Roll! {superEliteEmotes[num]}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Pink);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
            else if (roll == 100)
            {
                userAccount.LifetimeGambleWins++;
                userAccount.LifetimeGambles++;
                userAccount.LifetimeEliteRolls++;

                string sirenEmote = "<a:siren:429784681316220939>";

                var multiplier = 5.00;

                userAccount.Points += (uint)(points * multiplier);
                embed.WithTitle($"{sirenEmote} Gambling Winner: Perfect Roll! {sirenEmote}");
                embed.WithDescription($"**{user.Mention} rolled `{roll}` and won `{(points * multiplier).ToString("N0")}` points, `{multiplier}x` their bet!**\n" +
                    $"\nNew Average Chance of Elite+ Roll: **`{(userAccount.LifetimeEliteRolls / userAccount.LifetimeGambles).ToString("P")}`**");
                embed.WithFooter($"New Points Balance: {userAccount.Points.ToString("N0")} | Lifetime Gambles: {userAccount.LifetimeGambles} | " +
                    $"Average Lifetime Win Percent: {(userAccount.LifetimeGambleWins / userAccount.LifetimeGambles).ToString("P")}");
                embed.WithColor(Gold);
                BE();

                UserAccounts.SaveAccounts();
                return;
            }
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
