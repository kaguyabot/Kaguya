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

            var pages = ReturnPages();

            foreach (var cmd in cmdInfo.Commands.OrderBy(x => x.Name))
            {
                int i = 0;
                string aliases = cmd.Aliases.Where(alias => alias.ToLower() != cmd.Name.ToLower()).Aggregate("", (current, alias) => current + $"[{alias}]");
                foreach (var attr in attributes)
                {
                    string warn = "";
                    if (cmd.Attributes.Contains(attr) && attr.GetType() == typeof(DangerousCommandAttribute))
                        warn = @" {Dangerous Command}";

                    if (cmd.Attributes.Contains(attr) || cmd.Preconditions.Contains(attr))
                    {
                        if (!pages[i].Description.Contains($"{server.CommandPrefix}{cmd.Name.ToLower()} {aliases}{warn}\n"))
                        {
                            pages[i].Description += $"{server.CommandPrefix}{cmd.Name.ToLower()} {aliases}{warn}\n";
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
                pages[attributes.Length - 1] = null;

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
                    Title = "Command List: Administration (Page 1/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Currency (Page 2/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: EXP (Page 3/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Fun (Page 4/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Help (Page 5/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Music (Page 6/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: NSFW (Page 7/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: osu! (Page 8/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Utility (Page 9/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Supporter Only (Page 10/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Premium Servers Only (Page 11/11)",
                    Description = $"```css\n"
                },

                new PaginatedMessage.Page
                {
                    Title = "Command List: Owner Only (Page 12)",
                    Description = $"```css\n"
                }
            };

            return pages;
        }
    }
}
