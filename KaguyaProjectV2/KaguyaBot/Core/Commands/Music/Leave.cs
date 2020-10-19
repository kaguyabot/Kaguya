using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using Victoria;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Leave : KaguyaBase
    {
        [DisabledCommand]
        [MusicCommand]
        [Command("Leave")]
        [Summary("Makes Kaguya exit whatever voice channel it is currently in and " +
                 "disposes the current music player for the server.")]
        [Remarks("")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task Command()
        {
            Server server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            LavaNode node = ConfigProperties.LavaNode;
            SocketGuildUser curUser = Context.Guild.CurrentUser;
            SocketVoiceChannel curVc = curUser.VoiceChannel;
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
                                               $"command to work. Please try **joining a new voice channel** via " +
                                               $"`{server.CommandPrefix}join` if I am refusing to connect/disconnect.");
            }
        }
    }
}