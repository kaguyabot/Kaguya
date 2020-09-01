using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Premium
{
    public class CheckExpiration : KaguyaBase
    {
        [HelpCommand]
        [Command("CheckExpiration")]
        [Alias("ce")]
        [Summary("Allows a user with active Kaguya Premium keys " +
                 "to see when their subscription(s) will expire.")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var expiration = new KaguyaEmbedBuilder(EmbedColor.GOLD);

            if (await user.IsPremiumAsync())
            {
                var field = new EmbedFieldBuilder();
                field.Name = "Kaguya Premium";

                //todo: Figure out the user's active premium keys manually.
                foreach (var key in await user.GetActivePremiumKeysAsync())
                {
                    var guild = Client.GetGuild(key.ServerId);
                    field.Value += $"Server: `{(guild == null ? $"Unknown Server: (ID {key.ServerId})" : guild.Name)}`\n" +
                                   $"\tExpiration: `{DateTime.FromOADate(key.Expiration).Humanize(false)}`\n\n";
                }

                field.Value += $"User Benefits Expiration: `{DateTime.FromOADate(user.PremiumExpiration).Humanize(false)}`";
                expiration.AddField(field);
            }
            

            if (expiration.Fields.Count != 0)
            {
                await SendEmbedAsync(expiration);
            }
            else
            {
                await SendBasicErrorEmbedAsync($"You currently have no active subscriptions. You may " +
                                               $"purchase one [here]({ConfigProperties.KaguyaStore}).");
            }
        }
    }
}
