using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core.Command_Handler.EmbedHandlers;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using System;
using System.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Kaguya.Core.Embed;
using EmbedType = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Help
{
    public class Profile : InteractiveBase<SocketCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("profile")]
        [Alias("p")]
        public async Task ProfileCommand()
        {
            var user = Context.User;
            UserAccount account = UserAccounts.GetAccount(user);

            System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
            string monthName = mfi.GetMonthName(user.CreatedAt.Month).ToString();

            embed.WithTitle($"Kaguya Profile for {user.Username}");
            embed.AddField("User Information",
                $"User: `{user}`" +
                $"\nID: `{user.Id}`" +
                $"\nAccount Created: `{monthName} {user.CreatedAt.Day}, {user.CreatedAt.Year}`", true);
            embed.AddField("Kaguya Data",
                $"Points: `{account.Points.ToString("N0")}`" +
                $"\nEXP: `{account.EXP.ToString("N0")}`" +
                $"\nRep: `{account.Rep.ToString("N0")}`" +
                $"\nLevel: `{account.LevelNumber.ToString("N0")}`" +
                $"\n<a:KaguyaDiamonds:581562698228301876>: `{account.Diamonds.ToString("N0")}`" +
                $"\nLifetime Gambles: `{account.LifetimeGambles.ToString("N0")}`" +
                $"\nAverage Gamble Win %: `{(account.LifetimeGambleWins / account.LifetimeGambles * 100).ToString("N2")}%`" +
                $"\nAverage Elite+ Roll %: `{(account.LifetimeEliteRolls / account.LifetimeGambles * 100).ToString("N2")}%`", true);
            embed.WithThumbnailUrl(user.GetAvatarUrl());

            await BE();
        }
    }
}
