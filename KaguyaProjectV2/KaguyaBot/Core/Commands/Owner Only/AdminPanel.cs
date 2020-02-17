using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
// ReSharper disable OperatorIsCanBeUsed

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class AdminPanel : KaguyaBase
    {
        [OwnerCommand]
        [Command("ownerpanel", RunMode = RunMode.Async)]
        [Summary("Displays a control panel based on the user ID provided. " +
                 "This will display all stats about the user and allows a bot owner " +
                 "to modify these values.")]
        [Remarks("")]
        public async Task OwnerAdminPanel(IGuildUser user)
        {
            await AdminPanelMethod(user.Id);
        }

        [OwnerCommand]
        [Command("ownerpanel", RunMode = RunMode.Async)]
        [Summary("Displays a control panel based on the user ID provided. " +
                 "This will display all stats about the user and allows a bot owner " +
                 "to modify these values.")]
        [Remarks("")]
        public async Task OwnerAdminPanel(ulong Id)
        {
            await AdminPanelMethod(Id);
        }

        private async Task AdminPanelMethod(ulong Id)
        {
            KaguyaEmbedBuilder embed;
            var user = await DatabaseQueries.GetOrCreateUserAsync(Id);
            SocketUser socketUser = Client.GetUser(Id);

            embed = new KaguyaEmbedBuilder
            {
                Title = $"Data for {socketUser}",
                Fields = new List<EmbedFieldBuilder>()
            };

            Type t = user.GetType();

            foreach (PropertyInfo pi in t.GetProperties().Where(x => x.PropertyType == typeof(int) &&
                                                                     !x.Name.ToLower().Contains("active")))
            {
                string value = pi.GetValue(user, null)?.ToString() ?? "Null";

                if (pi.PropertyType == typeof(double))
                {
                    double.TryParse(pi.GetValue(user, null).ToString(), out double oaTime);
                    var dt = DateTime.FromOADate(DateTime.Now.ToOADate() - oaTime);
                    value = dt.Humanize();

                    if (DateTime.Now.Year - dt.Year >= 5) //More than 5 years ago? We don't care.
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

            await SendBasicSuccessEmbedAsync("What value would you like to modify?");
            var query = await NextMessageAsync(true, true, TimeSpan.FromSeconds(120));

            foreach (var prop in t.GetProperties())
            {
                if (query.Content.ToLower() == prop.Name.ToLower() && prop.CanWrite)
                {
                    await SendBasicSuccessEmbedAsync("What would you like to change the value to?");
                    var valMsg = await NextMessageAsync();

                    if (prop.PropertyType == typeof(int))
                        prop.SetValue(user, valMsg.Content.AsInteger(), null);
                    else
                    {
                        throw new InvalidOperationException($"The object's type cannot be parsed from this string. " +
                                                            $"Please review `AdminPanel.cs line 118`.");
                    }

                    await SendBasicSuccessEmbedAsync($"Successfully updated `{socketUser}`'s " +
                                                                     $"`{prop.Name}` value to `{valMsg.Content}`");
                }
            }
            await DatabaseQueries.UpdateAsync(user);
        }
    }
}
