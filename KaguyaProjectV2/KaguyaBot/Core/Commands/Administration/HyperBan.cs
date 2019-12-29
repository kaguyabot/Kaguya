﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class HyperBan : ModuleBase<ShardedCommandContext>
    {
        [SupporterCommand]
        [DangerousCommand]
        [AdminCommand]
        [Command("HyperBan")]
        [Summary("Permanently bans a user from this server and from **any other server " +
                 "that the command executor is an `Administrator` in.** I, (Kaguya) must be present in all mutual " +
                 "servers for this to work properly.")]
        [Remarks("<user>")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task DestroyThem(IGuildUser user)
        {
            await ActuallyBanThem(Context, user.Id);
        }

        [SupporterCommand]
        [DangerousCommand]
        [AdminCommand]
        [Command("HyperBan")]
        [Summary("Permanently bans a user from this server and from **any other server " +
                 "that the user is an `Administrator` in.** I must be present in all mutual " +
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
            var mutualGuilds = ((SocketUser) context.User).MutualGuilds.Where(x => 
                x.GetUser(context.User.Id).GuildPermissions.Administrator && 
                x.GetUser(Id) != null);
            var targets = new List<SocketGuildUser>();

            foreach(var guild in mutualGuilds)
            {
                targets.Add(guild.Users.FirstOrDefault(x => x.Id == Id));
            }

            foreach (var target in targets)
            {
                try
                {
                    await target.BanAsync(0, $"{context.User} used the `HyperBan` command " +
                                             $"in guild \"{context.Guild}\"");
                }
                catch (Exception)
                {
                    //
                }
            }

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"Successfully hyperbanned `{targets.FirstOrDefault()}` from " +
                              $"`{targets.Count().ToWords()}` servers. 😳"
            };

            await context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}