using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class ChannelBlacklist : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("ChannelBlacklist")]
        [Alias("cbl")]
        [Summary("Makes a channel blacklisted, disabling Kaguya completely in the channel. " +
                 "Kaguya will not respond to commands or " +
                 "post level-up announcements in this channel. Users with the `Administrator` server permission " +
                 "override this blacklist, and will not notice its effect. Level-up announcements will, however, " +
                 "still be disabled even if they are an `Administrator`.\n\n" +
                 "**Arguments:**\n\n" +
                 "The `-all` argument may be passed to completely disable Kaguya in the entire server.\n" +
                 "The `-clear` argument may be passed to lift all existing blacklists.\n\n" +
                 "To blacklist a specific channel, pass in it's `ID` or `Name`. If you don't know the ID (or " +
                 "the channel has an obscure name), you can simply use this command without an argument to " +
                 "blacklist the current channel immediately.")]
        [Remarks("(<= blacklists current channel)\n-all (<= blacklists all channels)\n" +
                 "-clear(<= removes all blacklists)\n<ID/Name> (<= blacklists a specific channel)")]
        public async Task Command(params string[] args)
        {

        }
    }
}
