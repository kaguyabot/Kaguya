using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Confirmation;
using Qommon.Collections;

namespace Kaguya.Discord.Overrides.Extensions
{
    internal static class KaguyaInteractivityExtensions
    {
        public static ReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> collection)
            => new ReadOnlyCollection<T>(collection.ToArray());

        public static ReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
            => new ReadOnlyDictionary<TKey, TValue>(dictionary);

        public static async Task<InteractivityResult<bool>> SendConfirmationAsync(this InteractivityService interactivityService,
            EmbedBuilder embed, ISocketMessageChannel channel, TimeSpan? timeout = null) =>  await SendConfirmationAsync(interactivityService, embed.Build(), channel, timeout);

        public static async Task<InteractivityResult<bool>> SendConfirmationAsync(this InteractivityService interactivityService,
            Embed embed, ISocketMessageChannel channel, TimeSpan? timeout = null)
        {
            var confirmation = new ConfirmationBuilder()
                               .WithContent(PageBuilder.FromEmbed(embed))
                               .Build();

            return await interactivityService.SendConfirmationAsync(confirmation, channel, timeout);
        }
    }
}