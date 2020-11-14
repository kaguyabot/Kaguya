using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Clear : KaguyaBase
    {
        [AdminCommand]
        [Command("Clear", RunMode = RunMode.Async)]
        [Alias("c", "purge")]
        [Summary("Clears the specified amount of messages from the current channel. " +
                 "If no value is specified, 10 messages will be cleared. A moderator " +
                 "may also specify a reason for clearing the messages. Premium servers " +
                 "will be able to have this reason be logged.")]
        [Remarks("\n<n>\n<n> <reason>")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ViewAuditLog)]
        public async Task ClearMessages(int amount = 10, string reason = null)
        {
            if (amount > 1000)
            {
                await SendBasicErrorEmbedAsync("You may not attempt to clear more than `1,000` messages " +
                                               "at a time.");

                return;
            }

            if (amount < 1)
            {
                await SendBasicErrorEmbedAsync("You must clear at least 1 message.");

                return;
            }

            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            KaguyaEmbedBuilder embed;

            server.IsCurrentlyPurgingMessages = true;
            await DatabaseQueries.UpdateAsync(server);

            IMessage[] messages = (await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync()).ToArray();
            IEnumerable<IMessage> invalidMessages = messages.Where(x => x.Timestamp.DateTime < DateTime.Now.AddDays(-14));

            await ((SocketTextChannel) Context.Channel).DeleteMessagesAsync(messages);

            // We take away 1 because the bot's own message is included in the collection.
            int msgDisplayCount = messages.Length - 1;
            string s = msgDisplayCount == 1 ? string.Empty : "s";
            if (!invalidMessages.Any())
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully deleted `{msgDisplayCount}` message{s}."
                };

                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(4));
            }
            else
            {
                embed = new KaguyaEmbedBuilder
                {
                    Description = $"Successfully deleted `{msgDisplayCount}` messages. Failed to delete " +
                                  $"`{amount - msgDisplayCount}` message{s}. This is likely because those " +
                                  "messages were posted more than two weeks ago. Messages posted more than two " +
                                  "weeks ago may not be deleted by Discord bots (this is a Discord-imposed limitation).",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "This message will be deleted in 15 seconds..."
                    }
                };

                embed.SetColor(EmbedColor.VIOLET);

                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(15));
            }

            server.IsCurrentlyPurgingMessages = false;
            await DatabaseQueries.UpdateAsync(server);
        }
    }
}