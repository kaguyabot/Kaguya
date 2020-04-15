using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class HyperBan : KaguyaBase
    {
        [PremiumCommand]
        [DangerousCommand]
        [Command("HyperBan")]
        [Summary("Permanently bans a user from this server and from **any other server " +
                 "that the command executor is an `Administrator` in.** Kaguya must be present in all mutual " +
                 "servers for this to work properly.")]
        [Remarks("<user>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task DestroyThem(IGuildUser user)
        {
            await ActuallyBanThem(Context, user.Id);
        }

        [PremiumCommand]
        [DangerousCommand]
        [AdminCommand]
        [Command("HyperBan")]
        [Summary("Permanently bans a user from this server and from **any other server " +
                 "that the command executor is an `Administrator` in.** Kaguya must be present in all mutual " +
                 "servers for this to work properly.")]
        [Remarks("<user>\n<ID>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task DestroyThem(ulong Id)
        {
            await ActuallyBanThem(Context, Id);
        }

        private async Task ActuallyBanThem(ICommandContext context, ulong Id)
        {
            var mutualGuilds = ((SocketUser)context.User).MutualGuilds.Where(x =>
               x.GetUser(context.User.Id).GuildPermissions.Administrator ||
               x.GetUser(context.User.Id) == x.Owner &&
               x.GetUser(Id) != null).ToList();
            var targets = new List<SocketGuildUser>();

            foreach (var guild in mutualGuilds)
            {
                var selection = guild.Users.FirstOrDefault(x => x.Id == Id);
                if (selection != null)
                    targets.Add(selection);
            }

            if (!targets.Any())
            {
                await SendBasicErrorEmbedAsync($"User is not present in any mutual servers.");
                return;
            }

            foreach (var target in targets)
            {
                try
                {
                    await target.BanAsync(0, $"{context.User} used the \"HyperBan\" command " +
                                             $"in guild \"{context.Guild}\"");
                }
                catch (Exception)
                {
                    //
                }
            }

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully hyperbanned `{targets.First()}` from " +
                              $"`{targets.Count.ToWords()}` servers. 😳"
            };

            await context.Channel.SendEmbedAsync(embed);
        }
    }
}