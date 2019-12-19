using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class AdminPanel : InteractiveBase<ShardedCommandContext>
    {
        [OwnerCommand]
        [Command("ownerpanel", RunMode = RunMode.Async)]
        [Summary("Displays a control panel based on the user ID provided. " +
                 "This will display all stats about the user and allows a bot owner " +
                 "to modify these values.")]
        [Remarks("")]
        public async Task OwnerAdminPanel(ulong Id)
        {
            KaguyaEmbedBuilder embed;
            var user = await UserQueries.GetOrCreateUser(Id);
            var socketUser = ConfigProperties.client.GetUser(Id);

            embed = new KaguyaEmbedBuilder
            {
                Title = $"Data for {socketUser}",
                Fields = new List<EmbedFieldBuilder>()
            };

            Type t = user.GetType();
            string[] skippedValues =
            {
                "total", "active", "latest", "given", "server",
                "expiration", "black", "supporter", "history"
            };

            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (skippedValues.Any(pi.Name.ToLower().Contains))
                    continue;

                string value = pi.GetValue(user, null)?.ToString() ?? "Null";

                if (pi.PropertyType == typeof(double))
                {
                    double.TryParse(pi.GetValue(user, null).ToString(), out double oaTime);
                    var dt = DateTime.FromOADate(DateTime.Now.ToOADate() - oaTime);
                    value = dt.Humanize();

                    if(DateTime.Now.Year - dt.Year >= 5) //More than 5 years ago? We don't care.
                    {
                        value = "Null";
                    }
                }

                embed.AddField(new EmbedFieldBuilder
                {
                    Name = pi.Name,
                    Value = $"`{value}`",
                    IsInline = true
                });
            }

            await ReplyAsync(embed: embed.Build());

            await ReplyAsync(embed: new KaguyaEmbedBuilder
            {
                Description = "What value would you like to modify?"
            }.Build());

            var query = await NextMessageAsync();

            switch (query.Content.ToLower())
            {
                case "experience":
                {
                    await ReplyAsync(embed: new KaguyaEmbedBuilder
                    {
                        Description = "What would you like to change the value to?"
                    }.Build());

                    var valMsg = await NextMessageAsync();
                    if (int.TryParse(valMsg.Content, out int val))
                    {
                        user.Experience = val;
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Successfully updated their `experience` value."
                        }.Build());
                    }
                    else
                    {
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Failed to parse value."
                        }.Build());
                    }
                    break;
                }
                case "points":
                {
                    await ReplyAsync(embed: new KaguyaEmbedBuilder
                    {
                        Description = "What would you like to change the value to?"
                    }.Build());

                    var valMsg = await NextMessageAsync();
                    if (int.TryParse(valMsg.Content, out int val))
                    {
                        user.Points = val;
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Successfully updated their `points` value."
                        }.Build());
                    }
                    else
                    {
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Failed to parse value."
                        }.Build());
                    }
                    break;
                }
                case "rep":
                {
                    await ReplyAsync(embed: new KaguyaEmbedBuilder
                    {
                        Description = "What would you like to change the value to?"
                    }.Build());

                    var valMsg = await NextMessageAsync();
                    if (int.TryParse(valMsg.Content, out int val))
                    {
                        user.Rep = val;
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Successfully updated their `rep` value."
                        }.Build());
                    }
                    else
                    {
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Failed to parse value."
                        }.Build());
                    }
                    break;
                }
                case "osuid":
                {
                    await ReplyAsync(embed: new KaguyaEmbedBuilder
                    {
                        Description = "What would you like to change the value to?"
                    }.Build());

                    var valMsg = await NextMessageAsync();
                    if (int.TryParse(valMsg.Content, out int val))
                    {
                        user.OsuId = val;
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Successfully updated their `osu! Id` value."
                        }.Build());
                    }
                    else
                    {
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Failed to parse value."
                        }.Build());
                    }
                    break;
                }
                case "ratelimitwarnings":
                {
                    await ReplyAsync(embed: new KaguyaEmbedBuilder
                    {
                        Description = "What would you like to change the value to?"
                    }.Build());

                    var valMsg = await NextMessageAsync();
                    if (int.TryParse(valMsg.Content, out int val))
                    {
                        user.RateLimitWarnings = val;
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Successfully updated their `ratelimit warnings` value."
                        }.Build());
                    }
                    else
                    {
                        await ReplyAsync(embed: new KaguyaEmbedBuilder
                        {
                            Description = $"Failed to parse value."
                        }.Build());
                    }
                    break;
                }
            }

            await UserQueries.UpdateUser(user);
        }
    }
}
