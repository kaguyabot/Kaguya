using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya.Core.CommandHandler
{
    public static class EmbedHandler
    {
        public static async Task<Embed> CreateBasicEmbed(string title, string description)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(252, 132, 255) //Pink
                .WithCurrentTimestamp().Build()));
            return embed;
        }

        public static async Task<Embed> CreateMusicEmbed(string title, string description)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(0, 255, 255) //Light Blue
                .WithCurrentTimestamp().Build()));
            return embed;
        }

        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithDescription($"**Error: {error}**")
                .WithColor(Color.Red)
                .WithCurrentTimestamp().Build());
            return embed;
        }
    }
}
