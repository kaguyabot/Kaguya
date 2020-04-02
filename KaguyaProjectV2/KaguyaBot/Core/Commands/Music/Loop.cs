using System.Text;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using Discord.WebSocket;
using Victoria.Enums;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using Humanizer;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Loop : KaguyaBase
    {
        [MusicCommand]
        [Command("Loop")]
        [Summary("Allows a user to repeat the song a certain number of times. Default is 1. " + 
                 "The song will be appended to the end of the queue as many times " + 
                 "as is specified, up to a maximum of 10.")]
        [Remarks("<number of times to repeat song>\n3")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        public async Task Command(int amount = 1)
        {
            if(amount > 10)
            {
                await SendBasicErrorEmbedAsync("The maximum amount of times a song may be re-queued at once is 10.");
                return;
            }

            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var player = ConfigProperties.LavaNode.GetPlayer(Context.Guild);

            var cmdPrefix = server.CommandPrefix;

            if(((SocketGuildUser)Context.User).VoiceChannel == null)
            {
                await SendBasicErrorEmbedAsync("You must be in a voice channel to use this command.");
                return;
            }

            if(player == null)
            {
                var eSb = new StringBuilder();
                eSb.AppendLine("There is no active music player for this server.");
                eSb.AppendLine($"Play some music with `{cmdPrefix}play` and `{cmdPrefix}search`.");
                await SendBasicErrorEmbedAsync(eSb.ToString());
                return;
            }

            if(player.PlayerState != PlayerState.Playing)
            {
                await SendBasicErrorEmbedAsync("The music player must be actively playing to use this command.");
                return;
            }

            var curTrack = player.Track;

            for(int i = 0; i < amount; i++)
            {
                player.Queue.Enqueue(curTrack);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Successfully enqueued track for looping.");
            sb.AppendLine("");
            sb.AppendLine($"Title: `{curTrack.Title}`");
            sb.AppendLine($"Duration: `{curTrack.Duration.Humanize(2)}`");
            sb.AppendLine($"Track Name: `{curTrack.Title}`");

            bool s = amount > 1;
            var embed = new KaguyaEmbedBuilder
            {
                Title = $"Loop Track {Centvrio.Emoji.Arrow.CounterClockwise}",
                Description = sb.ToString(),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Track will repeat {amount} additional time{(s ? "s" : "")}."
                }
            };

            await SendEmbedAsync(embed);
        }
    }
}