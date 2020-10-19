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
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class HyperBan : KaguyaBase
    {
        [PremiumUserCommand]
        [DangerousCommand]
        [Command("HyperBan")]
        [Summary("Permanently bans a user from this server and from **any other server " +
                 "that the command executor is an `Administrator` in.** Kaguya must be present in all mutual " +
                 "servers for this to work properly.")]
        [Remarks("<user>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task DestroyThem(IGuildUser user) => await ActuallyBanThem(Context, user.Id);

        [PremiumUserCommand]
        [DangerousCommand]
        [AdminCommand]
        [Command("HyperBan")]
        [Summary("Permanently bans a user from this server and from **any other server " +
            "that the command executor is an `Administrator` in.** Kaguya must be present in all mutual " +
            "servers for this to work properly.")]
        [Remarks("<user>\n<ID>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task DestroyThem(ulong id) => await ActuallyBanThem(Context, id);

        private async Task ActuallyBanThem(ICommandContext context, ulong id)
        {
            List<SocketGuild> mutualGuilds = ((SocketUser) context.User).MutualGuilds.Where(x =>
                x.GetUser(context.User.Id).GuildPermissions.Administrator ||
                x.GetUser(context.User.Id) == x.Owner &&
                x.GetUser(id) != null).ToList();

            var targets = new List<SocketGuildUser>();

            foreach (SocketGuild guild in mutualGuilds)
            {
                SocketGuildUser selection = guild.Users.FirstOrDefault(x => x.Id == id);
                if (selection != null)
                    targets.Add(selection);
            }

            if (!targets.Any())
            {
                await SendBasicErrorEmbedAsync($"User is not present in any mutual servers.");

                return;
            }

            foreach (SocketGuildUser target in targets)
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