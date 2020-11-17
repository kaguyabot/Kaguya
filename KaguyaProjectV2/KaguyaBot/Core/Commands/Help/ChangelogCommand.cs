using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Constants;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using LinqToDB.Common;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class ChangelogCommand : KaguyaBase
    {
        // The color we will use for all changelog embeds.
        private const EmbedColor EMBED_COLOR = EmbedColor.GREEN;

        [ReferenceCommand]
        [Command("Changelog")]
        [Alias("cl")]
        [Summary("Displays the changelog for the most recent update. Provide arguments to modify " +
                 "the response to your liking.\n\n" +
                 "__**Arguments:**__\n" +
                 "- `-n <num>` Will display the most recent changelogs up to the number specified. Executing " +
                 "with `-n 3` will display the 3 most recent changelogs.\n" +
                 "- `-v <version>` Will display the changelog for the exact version provided.\nExample: `v2.16`\n" +
                 "- `-ls` Will display all available changelog versions that can be searched through.\n\n" +
                 "**Note: These arguments may not be chained together.**")]
        [Remarks("\n[-n <num>]\n[-v <version num>]\n[-ls]")]
        public async Task Command(params string[] args)
        {
            string[] cl = await File.ReadAllLinesAsync(Path.Combine(FileConstants.RootDir, @"..\", "changelog.md"));
            KaguyaEmbedBuilder embed = await GenerateChangelogEmbed(cl, args);

            if (embed != null)
                await SendEmbedAsync(embed);
        }

        private async Task<KaguyaEmbedBuilder> GenerateChangelogEmbed(string[] clLines, params string[] args)
        {
            ChangelogArgs clArgs = await DetermineArgs(args);

            return clArgs switch
            {
                ChangelogArgs.DEFAULT => await RecentChangelogEmbed(clLines, null),
                ChangelogArgs.RECENT => await RecentChangelogEmbed(clLines, args),
                ChangelogArgs.VERSION_MATCH => await VersionSpecificChangelogEmbed(clLines, args),
                ChangelogArgs.LIST_VERSIONS => await ListVersionsEmbed(clLines),
                _ => throw new KaguyaSupportException("An unexpected error occurred.")
            };
        }

        private async Task<ChangelogArgs> DetermineArgs(params string[] args)
        {
            if (args.IsNullOrEmpty())
                return ChangelogArgs.DEFAULT;

            var ex = new KaguyaSupportException("I could not identify a way to parse the arguments you provided. " +
                                                "Please review this command's syntax (located in the `help changelog` comamnd)" +
                                                " for more information.");

            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            string p = server.CommandPrefix;
            string pS = p + "changelog";

            if (args.Length > 2)
            {
                throw new KaguyaSupportException($"To use this command with arguments, you must provide exactly 1 or 2 arguments.\n" +
                                                 $"Valid Examples: `{pS} -n 2`, `{pS} -i 3`, `{pS} -v 2.15`, `{pS} -ls`");
            }

            var regi = new List<Regex>
            {
                new Regex(@"-[N-n] [0-9]+"),
                new Regex(@"-[V-v]* [V-v]*[0-9]+.*"),
                new Regex(@"-[L-l][S-s]")
            };

            string argsStr = args.Humanize("").Replace("  ", " ");
            if (regi.Any(x => x.IsMatch(argsStr)))
            {
                int index = -1;

                for (int i = 0; i < regi.Count; i++)
                {
                    if (regi[i].IsMatch(argsStr))
                    {
                        index = i;

                        break;
                    }
                }

                return index switch
                {
                    0 => ChangelogArgs.RECENT,
                    1 => ChangelogArgs.VERSION_MATCH,
                    2 => ChangelogArgs.LIST_VERSIONS,
                    var _ => throw ex
                };
            }

            throw ex;
        }

        private async Task<KaguyaEmbedBuilder> RecentChangelogEmbed(string[] clLines, params string[] args)
        {
            if (args.IsNullOrEmpty())
            {
                string[] newArgs =
                {
                    "-n",
                    "1"
                };

                args = newArgs;
            }

            var titles = new List<string>
            {
                clLines[0]
            };

            var sb = new StringBuilder($"{Context.User.Mention}\n```\n");

            int versions = args[1].AsInteger();
            int versionsPassed = 0;

            for (int i = 0; i < clLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(clLines[i]))
                {
                    versionsPassed++;

                    // We can assume .AsInteger() will never error because the args are parsed 
                    // through a regex prior to this execution.
                    if (versionsPassed == versions)
                    {
                        sb.Append("```");

                        break;
                    }

                    // We index + 1 here because the title rests on the following line,
                    // not the current blank line.
                    titles.Add(clLines[i + 1]);
                }

                if (sb.Length > 2000)
                {
                    throw new KaguyaSupportException($"{Context.User.Mention} This request has too many characters to send " +
                                                     $"in chat! Try requesting fewer versions.\n\n" +
                                                     $"Or [view the full changelog online!](https://github.com/stageosu/Kaguya/blob/master/CHANGELOG.md)");
                }

                sb.AppendLine(clLines[i]);
            }

            if (titles.IsNullOrEmpty())
            {
                throw new KaguyaSupportException("Something went wrong when parsing the changelog.\n" +
                                                 "Error: Could not assign string title to a value in " +
                                                 "ChangelogCommand.cs.");
            }

            var titleSb = new StringBuilder();

            if (titles.Count == 1)
                titleSb.Append(titles[0]);
            else
                titleSb.Append($"{titles[0]} - {titles[^1]}");

            titleSb.Replace("### ", "");
            titleSb.Replace("Version ", "V");

            string titleString = titleSb.ToString();
            string descriptionString = sb.Replace("### ", "").ToString();

            var embed = new KaguyaEmbedBuilder(EMBED_COLOR)
            {
                Title = $"Kaguya Bot Changelog: {titleString}",
                Description = descriptionString
            };

            return embed;
        }

        private async Task<KaguyaEmbedBuilder> VersionSpecificChangelogEmbed(string[] clLines, params string[] args)
        {
            var descSb = new StringBuilder($"{Context.User.Mention}\n```");
            bool atVersion = false;
            string titleStr = string.Empty;

            foreach (string line in clLines)
            {
                string version = args[1];                                     // Examples: "v2.16, V2.16, 2.16"
                version = version.Replace('v', 'V').Replace("V", "Version "); // Ex: Version 2.16

                if (!version.Contains("Version "))
                    version = "Version " + version;

                string lReplace = line.Replace("### ", "");
                if (lReplace == version)
                {
                    atVersion = true;
                    titleStr = line;
                }

                if (atVersion)
                    descSb.AppendLine(line);

                // We have already determined we are at our version, 
                // but have now encountered a blank line. Therefore, 
                // we must be the end of this version's changelog.
                if (string.IsNullOrWhiteSpace(line) && atVersion)
                    break;
            }

            descSb.Append("```");
            descSb = descSb.Replace("###", "");
            titleStr = titleStr.Replace("###", "");

            if (string.IsNullOrWhiteSpace(titleStr))
            {
                await SendBasicErrorEmbedAsync("I could not find a match for " +
                                               $"`Version {args[1]}`.");

                return null;
            }

            return new KaguyaEmbedBuilder(EMBED_COLOR)
            {
                Title = titleStr,
                Description = descSb.ToString()
            };
        }

        private async Task<KaguyaEmbedBuilder> ListVersionsEmbed(string[] clLines)
        {
            var versionSb = new StringBuilder($"{Context.User.Mention}\n```");
            var versions = new List<string>();

            foreach (string line in clLines)
            {
                if (line.Contains("### Version "))
                    versions.Add(line.Replace("### Version ", ""));
            }

            versionSb.AppendLine(versions.Humanize(","));
            versionSb.Append("```");
            versionSb = versionSb.Replace("### Version ", "");
            versionSb = versionSb.Replace(", , ", ", "); // For some reason, we have to do this...

            return new KaguyaEmbedBuilder(EMBED_COLOR)
            {
                Title = "Kaguya Changelog History",
                Description = versionSb.ToString()
            };
        }
    }

    public enum ChangelogArgs
    {
        DEFAULT,
        RECENT,
        VERSION_MATCH,
        LIST_VERSIONS
    }
}