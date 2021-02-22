using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Music;
using Microsoft.Extensions.Logging;
using Victoria;

namespace Kaguya.Discord.Commands.Music
{
    [Module(CommandModule.Music)]
    [Group("shuffle")]
    public class Shuffle : KaguyaBase<Shuffle>
    {
        private readonly ILogger<Shuffle> _logger;
        private readonly LavaNode _lavaNode;
        public Shuffle(ILogger<Shuffle> logger, LavaNode lavaNode) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
        }

        [Command]
        [Summary("Shuffles the music queue in a random order.")]
        public async Task MusicShuffleCommandAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await SendBasicErrorEmbedAsync("The player couldn't be found for this server. Ensure " +
                                               "there is an active music player before using this command.");

                return;
            }

            if (!player.Queue.Any())
            {
                await SendBasicErrorEmbedAsync("There aren't any songs in the queue. Please add songs " +
                                               "to the queue with the `play` command and try again.");

                return;
            }

            player.Queue.Shuffle();

            string bonusDesc = MusicEmbeds.GetQueueEmbed(player.Queue, player.Track).Description;
            var embed = new KaguyaEmbedBuilder(KaguyaColors.LightOrange)
            {
                Title = "🔀 Queue Shuffle",
                Description = "Successfully shuffled the queue!\n\n" + bonusDesc
            };

            await SendEmbedAsync(embed);
        }
    }
}