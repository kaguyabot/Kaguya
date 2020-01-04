using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using Microsoft.Extensions.Logging;

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
                 "The `-clear` argument may be passed to lift all existing blacklists.\n" +
                 "The `-t` argument may be used to specify a time, and can be added to the other arguments as well! " +
                 "Seconds, minutes, hours, and even days may be passed in as an argument.\n\n" +
                 "To blacklist a specific channel, pass in it's `ID` or `Name`. If you don't know the ID (or " +
                 "the channel has an obscure name), you can simply use this command without an argument to " +
                 "blacklist the current channel immediately.")]
        [Remarks("(<= blacklists current channel)\n-all (<= blacklists all channels)\n" +
                 "-clear(<= removes all blacklists)\n<ID/Name> (<= blacklists a specific channel)\n" +
                 "-t 35m (<= blacklists current channel for 35 minutes)\n-all -t 12d36h (<= blacklists " +
                 "all channels for 12 days and 36 hours.)")]
        public async Task Command(params string[] _)
        {
            var args = _.ToList();
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var currentBlacklists = server.BlackListedChannels.ToList();

            bool hasT = false;
            bool hasAll = false;

            double expiration = DateTime.MaxValue.ToOADate();
            string expirationString = expiration == DateTime.MaxValue.ToOADate() ? "" : $"This blacklist will expire in " +
                                                                                        $"`{(DateTime.FromOADate(expiration) - DateTime.Now).Humanize()}`";
            if (args.Any(x => x.ToLower().Contains("-t")))
            {
                var tIndex = args.FindIndex(x => x.ToLower().Contains("-t"));
                TimeSpan ts = TimeSpan.MaxValue;
                try
                {
                    ts = args[tIndex + 1].ParseToTimespan();
                }
                catch (IndexOutOfRangeException)
                {
                    await Context.Channel.SendBasicErrorEmbedAsync($"Please specify a time after the `-t` argument.");
                }
                catch (Exception)
                {
                    throw new KaguyaSupportException("An error occurred when parsing the specified timespan. Either " +
                                                     "the timespan was too long, too short, or an invalid argument " +
                                                     "was passed.");
                }

                // Lib will reply in chat if any other exceptions occur, such as an invalid time input or too long, etc.

                if (ts.TotalDays > 365)
                    throw new InvalidOperationException("The duration value may not be over 365 days.");
                if (ts.TotalSeconds < 5)
                    throw new InvalidOperationException("The duration value must be at least 5.");

                expiration = (DateTime.Now + ts).ToOADate();
                hasT = true;

                args.RemoveAt(tIndex);
                args.RemoveAt(tIndex + 1);
            }

            // Since we set the "time args" to null above, we can now do normal checks :)
            if (args.Any(x => x.ToLower().Contains("-all")))
            {
                var allIndex = args.FindIndex(x => x.ToLower().Contains("-all"));
                args.RemoveAt(allIndex);
                hasAll = true;
            }

            if (!hasAll && args.Any(x => x.ToLower().Contains("-clear")))
            {
                if (args.Count > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(args), "You cannot specify more than " +
                                                                        "one argument if using `-clear`.");
                }

                await DatabaseQueries.ClearBlacklistedChannelsAsync(server);
                await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully cleared `{currentBlacklists.Count}` " +
                                                                 $"channels from the blacklist.");
                return;
            }

            if (args.Count == 0)
            {
                if (hasAll)
                {
                    foreach (var channel in Context.Guild.Channels)
                    {
                        var cbl = new BlackListedChannel
                        {
                            ServerId = Context.Guild.Id,
                            ChannelId = channel.Id,
                            Expiration = expiration
                        };

                        await DatabaseQueries.InsertAsync(cbl);
                    }

                    await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully blacklisted `{Context.Guild.Channels.Count}` " +
                                                                     $"channels. {expirationString}");
                }
                else
                {
                    var cbl = new BlackListedChannel
                    {
                        ServerId = Context.Guild.Id,
                        ChannelId = Context.Channel.Id,
                        Expiration = expiration
                    };

                    await DatabaseQueries.InsertAsync(cbl);
                    await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully blacklisted this channel. {expirationString}");
                }
            }
        }
    }
}
