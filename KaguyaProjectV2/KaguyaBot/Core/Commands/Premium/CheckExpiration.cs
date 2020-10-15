using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Premium
{
    public class CheckExpiration : KaguyaBase
    {
        [ReferenceCommand]
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

            if (user.IsPremium)
            {
                var field = new EmbedFieldBuilder {Name = "Kaguya Premium"};

                var includedServers = new List<Server>();
                foreach (var key in await DatabaseQueries.GetAllForUserAsync<PremiumKey>(user.UserId, x => x.ServerId != 0))
                {
                    var guild = Client.GetGuild(key.ServerId);
                    var server = await DatabaseQueries.GetOrCreateServerAsync(key.ServerId);

                    if (includedServers.Any(x => x.ServerId == server.ServerId) || server.PremiumExpiration < DateTime.Now.ToOADate())
                        continue;
                    
                    field.Value += $"Server: `{(guild == null ? $"Unknown Server: (ID {key.ServerId})" : guild.Name)}`\n" +
                                   $"\tExpiration: `{DateTime.FromOADate(server.PremiumExpiration):MMMM dd, yyyy}`\n\n";
                    
                    includedServers.Add(server); // Skips duplicates
                }

                field.Value += $"User Benefits Expiration: `{DateTime.FromOADate(user.PremiumExpiration):MMMM dd, yyyy}`";
                expiration.AddField(field);
            }
            
            if (expiration.Fields.Count != 0)
            {
                await SendEmbedAsync(expiration);
            }
            else
            {
                await SendBasicErrorEmbedAsync($"You currently have no active subscriptions. You may " +
                                               $"purchase one [here]({ConfigProperties.KaguyaStoreURL}).");
            }
        }
    }
}
