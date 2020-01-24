using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Move : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("Move")]
        [Summary("Moves one user to the specified voice channel. " +
                 "The channel name or ID are both valid parameters for the `voice channel` parameter.\n\n" +
                 "*The channel's name doesn't have to be an exact match, as long as it's close ;). Shoutout to servers " +
                 "who use emojis in the channel names :D*")]
        [Remarks("<user> <voice channel>")]
        [RequireUserPermission(GuildPermission.MoveMembers)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        public async Task Command(SocketGuildUser user, [Remainder]string voiceChannel)
        {
            await MoveUsersToVoiceChannel(new List<SocketGuildUser> {user}, voiceChannel);
        }

        [PremiumServerCommand]
        [AdminCommand]
        [Command("MoveSplit")]
        [Summary("Evenly divides all users from the current voice channel into all voice channels that match the provided " +
                 "input. The input is a series of characters that the voice channels' names must all have.\n" +
                 "Example:\n\n" +
                 "If I have 30 users and 3 voice channels containing the word `team`: `Team 1`, `Team 2`, and `Team 3`, " +
                 "I can evenly split all users from this voice channel into those three channels by using `split team`.")]
        public async Task MoveSplit([Remainder] string matchVoiceChannel)
        {
            if (!(Context.User is SocketGuildUser guildUser))
            {
                throw new KaguyaSupportException("Unable to identify the user as a `SocketGuildUser`. If this command was executed " +
                                                 "from a valid Discord server, please contact support in the provided Discord server.");
            }

            // ReSharper disable once PossibleNullReferenceException
            var curVc = (Context.User as SocketGuildUser).VoiceChannel;

            if(curVc.Users.Count > 70)
                throw new KaguyaSupportException("Sorry, but this command is limited to 70 users being in the voice channel at once.");

            await MoveUsersToVoiceChannel(curVc.Users.ToList(), matchVoiceChannel);
        }

        private async Task MoveUsersToVoiceChannel(SocketGuildUser user, string voiceChannel)
        {
            var currentChannel = user.VoiceChannel;
            SocketVoiceChannel destinationChannel = null;
            try
            {
                destinationChannel = voiceChannel.AsUlong(false) != 0
                    ? Context.Guild.VoiceChannels.First(x => x.Id == voiceChannel.AsUlong())
                    : Context.Guild.VoiceChannels.First(x => x.Name.ToLower().Contains(voiceChannel.ToLower()));
            }
            catch (InvalidOperationException)
            {
                // Leaving this blank because leaving destinationChannel as null triggers more helpful exceptions.
            }

            if (currentChannel == null)
                throw new KaguyaSupportException(
                    "The user must be present in a voice channel in order for this command to work.");
            if (destinationChannel == null)
                throw new KaguyaSupportException("The voice channel you specified either doesn't exist in the server or " +
                                                 "I don't have permissions to access it.");
            if (destinationChannel == currentChannel)
            {
                await Context.Channel.SendBasicErrorEmbedAsync($"The user is already in this voice channel.");
                return;
            }

            await user.ModifyAsync(x => x.Channel = destinationChannel);
            await Context.Channel.SendBasicSuccessEmbedAsync($"User `{user}` has been moved from `{currentChannel.Name}` " +
                                                             $"into `{destinationChannel.Name}`.");
        }

        private async Task MoveUsersToVoiceChannel(List<SocketGuildUser> users, string destinationChannelMatch)
        {
            var channels = Context.Guild.VoiceChannels.Where(x => x.Name.ToLower().Contains(destinationChannelMatch)).ToList();
            int count = (int)Math.Ceiling((double)users.Count / channels.Count);

            int i = 0;
            foreach(var channel in channels)
            {
                for (int j = 0; j < count; j++)
                {
                    try
                    {
                        var curUser = users[j + (i * 10)];
                        await curUser.ModifyAsync(x => x.Channel = channel);
                        await ReplyAsync($"`{curUser}` has been moved to `{channel.Name}`");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return;
                    }
                }

                i++;
            }
        }
    }
}
