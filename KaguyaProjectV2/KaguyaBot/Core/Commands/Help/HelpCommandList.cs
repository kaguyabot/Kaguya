using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Handlers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class HelpCommandList : KaguyaBase
    {
        [Command("Help")]
        [Alias("h")]
        public async Task Help()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var cmdInfo = CommandHandler._commands;

            var attributes = new Attribute[]
            {
                new AdminCommandAttribute(), new CurrencyCommandAttribute(),
                new ExpCommandAttribute(), new FunCommandAttribute(),
                new HelpCommandAttribute(), new MusicCommandAttribute(),
                new NsfwCommandAttribute(), new OsuCommandAttribute(),
                new UtilityCommandAttribute(), new PremiumCommandAttribute(),
                new OwnerCommandAttribute()
            };

            var pages = ReturnPages().ToList();

            foreach (var cmd in cmdInfo.Commands.OrderBy(x => x.Name))
            {
                int i = 0;
                string aliases = cmd.Aliases.Where(alias => alias.ToLower() != cmd.Name.ToLower())
                    .Aggregate("", (current, alias) => current + $"[{alias}]");
                foreach (var attr in attributes)
                {
                    // We skip this attribute because it no longer has a page of its own.
                    if (attr.GetType() == typeof(PremiumCommandAttribute))
                        continue;
                    
                    string warn = "";
                    string premium = "";
                    
                    if (cmd.Preconditions.Contains(attributes[9]))
                        premium = "{$}";
                    
                    if (cmd.Attributes.Contains(attr) || cmd.Preconditions.Contains(attr))
                    {
                        if (!pages[i].Description.Contains($"{server.CommandPrefix}{cmd.Name.ToLower()} {aliases}{warn}"))
                        {
                            if (!string.IsNullOrWhiteSpace(aliases))
                            {
                                aliases = aliases.Insert(0, " ");
                            }
                            
                            pages[i].Description += $"{server.CommandPrefix}{cmd.Name.ToLower()}{aliases} {premium}";
                            pages[i].Description += "\n";
                        }
                    }

                    i++;
                }
            }

            foreach (var pg in pages)
            {
                pg.Description += "```";
            }

            if (Context.User.Id != ConfigProperties.BotConfig.BotOwnerId)
                pages.RemoveAt(pages.Count - 1);

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

        private IEnumerable<PaginatedMessage.Page> ReturnPages()
        {
            var pages = new[]
            {
                new PaginatedMessage.Page
                {
                    Title = "Command List: Administration (Page 1/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Currency (Page 2/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: EXP (Page 3/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Fun (Page 4/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Help (Page 5/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Music (Page 6/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: NSFW (Page 7/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: osu! (Page 8/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Utility (Page 9/9)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Owner Only (Page 10: Hidden)",
                    Description = $"```css\n"
                }
            };

            return pages;
        }
    }
}
