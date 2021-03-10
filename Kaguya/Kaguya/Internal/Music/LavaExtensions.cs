using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria;

namespace Kaguya.Internal.Music
{
	public static class LavaExtensions
	{
		public static async Task<bool> SafeJoinAsync(this LavaNode lavaNode,
			SocketUser user,
			ISocketMessageChannel channel)
		{
			if (user is not SocketGuildUser guildUser || channel is not ITextChannel textChannel)
			{
				return false;
			}

			var voiceChannel = ((IVoiceState) guildUser).VoiceChannel;
			if (voiceChannel == null)
			{
				return false;
			}

			if (!lavaNode.HasPlayer(guildUser.Guild))
			{
				try
				{
					await (await lavaNode.JoinAsync(voiceChannel, textChannel)).UpdateVolumeAsync(100);
				}
				catch (Exception)
				{
					return false;
				}
			}

			return true;
		}
	}
}