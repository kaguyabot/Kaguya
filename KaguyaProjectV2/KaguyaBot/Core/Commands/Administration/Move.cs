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
    public class Move : KaguyaBase
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
                await SendBasicErrorEmbedAsync($"The user is already in this voice channel.");
                return;
            }

            await user.ModifyAsync(x => x.Channel = destinationChannel);
            await SendBasicSuccessEmbedAsync($"User `{user}` has been moved from `{currentChannel.Name}` " +
                                                             $"into `{destinationChannel.Name}`.");
        }
    }
}
