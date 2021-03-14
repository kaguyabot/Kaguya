using Discord;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Confirmation;
using Qommon.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kaguya.Discord.Overrides.Extensions
{
	internal static class KaguyaInteractivityExtensions
	{
		public static ReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> collection)
		{
			return new(collection.ToArray());
		}

		public static ReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(
			this Dictionary<TKey, TValue> dictionary)
		{
			return new(dictionary);
		}

		public static async Task<InteractivityResult<bool>> SendConfirmationAsync(
			this InteractivityService interactivityService, EmbedBuilder embed, ISocketMessageChannel channel,
			TimeSpan? timeout = null)
		{
			return await SendConfirmationAsync(interactivityService, embed.Build(), channel, timeout);
		}

		public static async Task<InteractivityResult<bool>> SendConfirmationAsync(
			this InteractivityService interactivityService, Embed embed, ISocketMessageChannel channel,
			TimeSpan? timeout = null)
		{
			var confirmation = new ConfirmationBuilder().WithContent(PageBuilder.FromEmbed(embed)).Build();

			return await interactivityService.SendConfirmationAsync(confirmation, channel, timeout);
		}
	}
}