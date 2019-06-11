using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Attributes;
using Kaguya.Core.Embed;
using Kaguya.Core.Server_Files;
using System.Diagnostics;
using System.Threading.Tasks;
using EmbedColor = Kaguya.Core.Embed.EmbedColor;

namespace Kaguya.Modules.Administration
{
    [KaguyaModule("Administration")]
    public class ChannelWhitelistBlacklist : InteractiveBase<ShardedCommandContext>
    {
        public KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();
        readonly DiscordShardedClient _client = Global.client;

        public async Task BE() //Method to build and send an embedded message.
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("channelblacklist")]
        [Alias("cbl")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)] //Not really needed but just to be safe.
        public async Task ChannelBlacklist(IGuildChannel channel)
        {
            var server = Servers.GetServer(Context.Guild);
            var blacklist = server.BlacklistedChannels;
            var whitelist = server.WhitelistedChannels;

            blacklist.Add(channel.Id);
            whitelist.Clear();
            Servers.SaveServers();

            embed.WithTitle($"Channel Blacklist");
            embed.WithDescription($"{Context.User.Mention} I have blacklisted the channel #`{channel.Name}`.");
            embed.SetColor(EmbedColor.VIOLET);
            embed.WithFooter($"The server's channel whitelist has been cleared as a result.");
            await BE();
        }

        [Command("channelwhitelist")]
        [Alias("cwl")]
        public async Task ChannelWhiteList(IGuildChannel channel)
        {
            var server = Servers.GetServer(Context.Guild);
            var blacklist = server.BlacklistedChannels;
            var whitelist = server.WhitelistedChannels;

            whitelist.Add(channel.Id);
            blacklist.Clear();
            Servers.SaveServers();

            embed.WithTitle($"Channel Whitelist");
            embed.WithDescription($"I have whitelisted the channel `#{channel.Name}`");
            embed.SetColor(EmbedColor.VIOLET);
            embed.WithFooter($"This server's blacklist has been cleared as a result.");
            await BE();
        }

        [Command("channelunblacklist")]
        [Alias("cubl")]
        public async Task ChannelUnBlackList(IGuildChannel channel)
        {
            var server = Servers.GetServer(Context.Guild);
            var blacklist = server.BlacklistedChannels;

            if (!blacklist.Contains(channel.Id))
            {
                embed.WithTitle($"Error: Channel Un-Whitelist");
                embed.WithDescription($"This channel isn't in the whitelist!");
                embed.SetColor(EmbedColor.RED);
                await BE(); return;
            }

            blacklist.Remove(channel.Id);
            Servers.SaveServers();

            if (blacklist.Count < 1)
            {
                embed.WithTitle($"Error: Channel Un-Blacklist");
                embed.WithDescription($"There aren't any channels to un-blacklist!");
                embed.SetColor(EmbedColor.RED);
                await BE();
            }
            else if (blacklist.Count == 1)
            {
                blacklist.Clear();
                Servers.SaveServers();

                embed.WithTitle($"Channel Un-Blacklisted");
                embed.WithDescription($"This was the last channel in the blacklist, so the blacklist has been lifted!");
                embed.WithFooter($"All channels may now use my commands.");
                embed.SetColor(EmbedColor.RED);
                await BE();
            }
            else if (blacklist.Count > 0)
            {
                embed.WithTitle($"Channel Un-Blacklist");
                embed.WithDescription($"I have un-blacklisted the channel `#{channel.Name}`");
                embed.SetColor(EmbedColor.VIOLET);
                await BE();

                blacklist.Remove(channel.Id);
                Servers.SaveServers();
            }
        }

        [Command("channelunwhitelist")]
        [Alias("cuwl")]
        public async Task ChannelUnWhiteList(IGuildChannel channel)
        {
            var server = Servers.GetServer(Context.Guild);
            var whitelist = server.WhitelistedChannels;

            if (!whitelist.Contains(channel.Id))
            {
                embed.WithTitle($"Error: Channel Un-Whitelist");
                embed.WithDescription($"This channel isn't in the whitelist!");
                embed.SetColor(EmbedColor.RED);
                await BE(); return;
            }

            if (whitelist.Count == 1)
            {
                whitelist.Clear();
                Servers.SaveServers();

                embed.WithTitle($"Channel Un-Whitelist");
                embed.WithDescription($"This was the last channel in the whitelist, so the whitelist has been lifted!");
                embed.WithFooter($"All channels may now use my commands.");
                embed.SetColor(EmbedColor.RED);
                await BE();
            }
            else if (whitelist.Count < 1)
            {
                embed.WithTitle($"Error: Channel Un-Whitelist");
                embed.WithDescription($"There aren't any channels to un-whitelist!");
                embed.SetColor(EmbedColor.RED);
                await BE(); return;
            }
            else if (whitelist.Count > 1)
            {
                embed.WithTitle($"Channel Un-Whitelist");
                embed.WithDescription($"I have un-whitelisted the channel `#{channel.Name}`");
                embed.SetColor(EmbedColor.VIOLET);
                await BE();

                whitelist.Remove(channel.Id);
                Servers.SaveServers();
            }
        }

        [Command("blacklist")]
        [Alias("bl")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ViewBlacklist()
        {
            Server server = Servers.GetServer(Context.Guild);
            var blacklist = server.BlacklistedChannels;

            string blacklistedChannels = "";

            foreach(var channel in blacklist)
            {
                blacklistedChannels += $"`#{_client.GetChannel(channel).ToString()}`\n";
            }

            embed.WithTitle($"Channel Blacklist");
            embed.WithDescription($"Channel Blacklist for `{Context.Guild.Name}`" +
                $"\n" +
                $"\n{blacklistedChannels}");
            await BE();

        }

        [Command("whitelist")]
        [Alias("wl")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ViewWhiteList()
        {
            Server server = Servers.GetServer(Context.Guild);
            var whitelist = server.WhitelistedChannels;

            string whitelistedChannels = "";

            foreach (var channel in whitelist)
            {
                whitelistedChannels += $"`#{_client.GetChannel(channel).ToString()}`\n";
            }

            embed.WithTitle($"Channel Whitelist");
            embed.WithDescription($"Channel Whitelist for `{Context.Guild.Name}`" +
                $"\n" +
                $"\n{whitelistedChannels}");
            await BE();
        }
    }
}
