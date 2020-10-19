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
using System.Text;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class HelpCommandList : KaguyaBase
    {
        [Command("Help")]
        [Alias("h")]
        public async Task Help()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            CommandService cmdInfo = CommandHandler.Commands;

            var attributes = new Attribute[]
            {
                new AdminCommandAttribute(),
                new CurrencyCommandAttribute(),
                new ExpCommandAttribute(),
                new FunCommandAttribute(),
                new ReferenceCommandAttribute(),
                new MusicCommandAttribute(),
                new NsfwCommandAttribute(),
                new OsuCommandAttribute(),
                new UtilityCommandAttribute(),
                new PremiumServerCommandAttribute(),
                new PremiumUserCommandAttribute(),
                new OwnerCommandAttribute()
            };

            List<PaginatedMessage.Page> pages = ReturnPages(server.CommandPrefix).ToList();

            foreach (CommandInfo cmd in cmdInfo.Commands.OrderBy(x => x.Name))
            {
                int i = 0;
                string aliases = cmd.Aliases.Where(alias => alias.ToLower() != cmd.Name.ToLower())
                                    .Aggregate("", (current, alias) => current + $"[{alias}]");

                foreach (Attribute attr in attributes)
                {
                    // We skip this attribute because it no longer has a page of its own.
                    if (attr.GetType() == typeof(PremiumUserCommandAttribute) || attr.GetType() == typeof(PremiumServerCommandAttribute))
                        continue;

                    string dangerous = "";
                    string premium = "";
                    string disabled = "";

                    if (cmd.Preconditions.Any(x => x.GetType() == typeof(DangerousCommandAttribute)))
                        dangerous = "(Dangerous)";

                    if (cmd.Preconditions.Contains(attributes[9]) || cmd.Preconditions.Contains(attributes[10]))
                        premium = "{$}";

                    if (cmd.Preconditions.Any(x => x.GetType() == typeof(DisabledCommandAttribute)))
                        disabled = "<DISABLED>";

                    if (cmd.Attributes.Contains(attr) || cmd.Preconditions.Contains(attr))
                    {
                        if (!pages[i].Description.Contains($"{server.CommandPrefix}{cmd.Name.ToLower()} {aliases}"))
                        {
                            if (!string.IsNullOrWhiteSpace(aliases))
                                aliases = aliases.Insert(0, " ");

                            // All commands are displayed using this String Builder.
                            // Future command parameters
                            var descSb = new StringBuilder($"{server.CommandPrefix}{cmd.Name.ToLower()}{aliases}");

                            if (!string.IsNullOrWhiteSpace(dangerous))
                                descSb.Append(" " + dangerous);

                            if (!string.IsNullOrWhiteSpace(premium))
                                descSb.Append(" " + premium);

                            if (!string.IsNullOrWhiteSpace(disabled))
                                descSb.Append(" " + disabled);

                            pages[i].Description += descSb.ToString();
                            pages[i].Description += "\n";
                        }
                    }

                    i++;
                }
            }

            foreach (PaginatedMessage.Page pg in pages)
                pg.Description += "```";

            if (Context.User.Id != ConfigProperties.BotConfig.BotOwnerId)
                pages.RemoveAt(pages.Count - 1);

            var pager = new PaginatedMessage
            {
                Pages = pages,
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

        private IEnumerable<PaginatedMessage.Page> ReturnPages(string cmdPrefix)
        {
            var pages = new[]
            {
                new PaginatedMessage.Page
                {
                    Title = "Command List: Administration (Page 1/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: Currency (Page 2/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: EXP (Page 3/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: Fun (Page 4/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: Reference (Page 5/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: Music (Page 6/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: NSFW (Page 7/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: osu! (Page 8/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: Utility (Page 9/9)",
                    Description = "```css\n"
                },
                new PaginatedMessage.Page
                {
                    Title = "Command List: Owner Only (Page 10: Hidden)",
                    Description = "```css\n"
                }
            };

            // Additional description information that is applied to ALL command pages.
            foreach (PaginatedMessage.Page page in pages)
            {
                string descCopy = page.Description;
                page.Description = ""; // Resetting the description.

                var descSb = new StringBuilder();
                descSb.AppendLine($"- Use **`{cmdPrefix}h <command>`** for additional command information.");
                descSb.AppendLine("- A **`{$}`** icon indicates the command is for premium users.");
                descSb.AppendLine("- A **`(Dangerous)`** indicator indicates the command should be executed with caution.");
                descSb.AppendLine();
                descSb.AppendLine("Helpful Links:".ToDiscordBold());
                descSb.Append($"*[Kaguya Support]({ConfigProperties.KAGUYA_SUPPORT_DISCORD_URL})*, ");
                descSb.Append($"*[Kaguya Premium]({ConfigProperties.KAGUYA_STORE_URL})*, ");
                descSb.Append($"*[Invite Kaguya]({ConfigProperties.KAGUYA_INVITE_URL})*");

                page.Description = descSb + "\n" + descCopy;
            }

            return pages;
        }
    }
}