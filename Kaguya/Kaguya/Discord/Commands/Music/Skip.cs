using System;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Interactivity;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Music;
using Victoria;

namespace Kaguya.Discord.Commands.Music
{
    [Module(CommandModule.Music)]
    [Group("skip")]
    [RequireUserPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Speak)]
    public class Skip : KaguyaBase<Skip>
    {
        private readonly ILogger<Skip> _logger;
        private readonly LavaNode _lavaNode;
        private readonly InteractivityService _interactivityService;

        public Skip(ILogger<Skip> logger, LavaNode lavaNode, InteractivityService interactivityService) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
            _interactivityService = interactivityService;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("Skips the current song. Pass in a number to skip multiple songs at once.")]
        [Remarks("[# skips]")]
        [Example("")]
        [Example("2 (skips current + next song)")]
        [Example("3 (skips current + next 2 songs)")]
        public async Task SkipCommand(int? skipCount = null)
        {
            if (skipCount < 2)
            {
                await SendBasicErrorEmbedAsync("If you are specifiying a skip count, you must skip 2+ tracks.");

                return;
            }
            
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                var embed = GetBasicErrorEmbedBuilder("It doesn't look like there is an active player. Play some songs to use this command.").Build();
                _interactivityService.SendEmbedWithDeletion(Context, embed, TimeSpan.FromSeconds(15));

                return;
            }

            LavaTrack curTrack = player.Track;
            bool emptyQueue = player.Queue.Count == 0;
            if (curTrack == null)
            {
                var embed =  GetBasicErrorEmbedBuilder("There isn't anything to skip.").Build();
                _interactivityService.SendEmbedWithDeletion(Context, embed, TimeSpan.FromSeconds(15));

                return;
            }

            if (emptyQueue)
            {
                await player.StopAsync();
                var embed = GetBasicEmbedBuilder($"Skipped {curTrack.Title.AsBold()}. No more tracks remaining.", Color.Purple).Build();
                _interactivityService.SendEmbedWithDeletion(Context, embed, TimeSpan.FromSeconds(15));
            }
            else if (!skipCount.HasValue || skipCount.Value == 1)
            {
                await player.SkipAsync();

                var embed = GetBasicSuccessEmbedBuilder($"Skipped {curTrack.Title.AsBold()}").Build();
                _interactivityService.SendEmbedWithDeletion(Context, embed, TimeSpan.FromSeconds(15));
                
                _interactivityService.SendEmbedWithDeletion(Context, MusicEmbeds.GetNowPlayingEmbedForTrack(player.Track), TimeSpan.FromSeconds(15));
            }
            else
            {
                int actualSkipCount = 0;
                for (int i = 0; i < skipCount.Value; i++)
                {
                    try
                    {
                        await player.SkipAsync();
                        actualSkipCount++;
                    }
                    catch (InvalidOperationException)
                    {
                        await player.StopAsync();
                        break;
                    }
                }

                string s = actualSkipCount == 1 ? "" : "s";
                await SendBasicSuccessEmbedAsync($"Skipped {actualSkipCount:N0} track{s}.");
            }
        }
    }
}