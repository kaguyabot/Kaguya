using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Leave : KaguyaBase
    {
        [MusicCommand]
        [Command("Leave")]
        [Summary("Makes Kaguya exit whatever voice channel it is currently in and " +
                 "disposes the current music player for the server.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task Command()
        {
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var node = ConfigProperties.LavaNode;
            var curUser = Context.Guild.CurrentUser;
            var curVc = curUser.VoiceChannel;
            if (node.HasPlayer(Context.Guild))
            {
                if (curVc == null)
                {
                    await SendBasicErrorEmbedAsync($"{Context.User.Mention} Please ensure I " +
                                                                   $"am actively in a voice channel before using " +
                                                                   $"this command.");
                }
                else
                {
                    try
                    {
                        await node.LeaveAsync(curVc);
                    }
                    catch (Exception)
                    {
                        throw new KaguyaSupportException("Error when disconnecting from voice channel:\n\n" +
                                                         $"Channel name: `{curVc.Name}`\n" +
                                                         $"Users connected (including Kaguya): `{curVc.Users.Count:N0}`\n\n" +
                                                         $"Develper Note: `Please report this to the provided " +
                                                         $"Discord server and contact Stage. Thanks!`");
                    }

                    await SendBasicSuccessEmbedAsync(
                        $"{Context.User.Mention} Successfully disconnected from `{curVc.Name}`.");
                }
            }
            else
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} I must be in a voice channel via " +
                                                               $"the `{server.CommandPrefix}join` command for this " +
                                                               $"command to work. Please try `{server.CommandPrefix}join` " +
                                                               $"and then `{server.CommandPrefix}leave` again to remove " +
                                                               $"me from the voice channel if I am stuck.");
            }
        }
    }
}
