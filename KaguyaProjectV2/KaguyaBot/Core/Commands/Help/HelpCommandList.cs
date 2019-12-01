using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class HelpCommandList : InteractiveBase<ShardedCommandContext>
    {
        [Command("Help")]
        [Alias("h")]
        public async Task Help()
        {
            Server server = ServerQueries.GetServer(Context.Guild.Id);
            var cmdInfo = CommandHandler._commands;

            Attribute[] attributes = new Attribute[]
            {
                new AdminCommandAttribute(), new CurrencyCommandAttribute(),
                new ExpCommandAttribute(), new FunCommandAttribute(), 
                new HelpCommandAttribute(), new MusicCommandAttribute(), 
                new NsfwCommandAttribute(), new OsuCommandAttribute(), 
                new UtilityCommandAttribute(), new SupporterCommandAttribute(), 
                new RequireOwnerAttribute()
            };

            var pages = ReturnPages();

            foreach (var cmd in cmdInfo.Commands.OrderBy(x => x.Name))
            {
                int i = 0;
                string aliases = cmd.Aliases.Where(alias => alias.ToLower() != cmd.Name.ToLower()).Aggregate("", (current, alias) => current + $"[{alias}]");
                foreach(var attr in attributes)
                {
                    if (cmd.Attributes.Contains(attr) || cmd.Preconditions.Contains(attr))
                    {
                        pages[i]
                            .Description += $"{server.CommandPrefix}{cmd.Name.ToLower()} {aliases}\n";
                    }

                    i++;
                }
            }

            foreach (var pg in pages)
            {
                pg.Description += "```";
            }

            if (Context.User.Id != 146092837723832320)
                pages[10] = null;

            var pager = new PaginatedMessage
            {
                Pages = pages,
                FooterOverride = new EmbedFooterBuilder
                {
                    Text = $"Use {server.CommandPrefix}h <command> for more information on a command.\n" +
                           $"Press the square numbers and then type a number in chat to jump to a page."
                },
                Color = KaguyaEmbedBuilder.BlueColor
            };

            await PagedReplyAsync(pager, new ReactionList
            {
                First = true,
                Last = true,
                Forward = true,
                Backward = true,
                Jump = true
            });
        }

        private PaginatedMessage.Page[] ReturnPages()
        {
            var pages = new[]
            {
                new PaginatedMessage.Page
                {
                    Title = "Command List: Administration (Page 1/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Currency (Page 2/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: EXP (Page 3/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Fun (Page 4/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Help (Page 5/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Music (Page 6/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: NSFW (Page 7/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: osu! (Page 8/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Utility (Page 9/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Supporter Only (Page 10/10)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Owner Only (Page 11)",
                    Description = $"```css\n"
                }
            };

            return pages;
        }
    }
}
