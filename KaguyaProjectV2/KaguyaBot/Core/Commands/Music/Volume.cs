using System;
using System.Linq;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System.Threading.Tasks;
using Discord;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Music
{
    public class Volume : KaguyaBase
    {
        [MusicCommand]
        [Command("Volume")]
        [Alias("v", "vol")]
        [Summary("Allows you to adjust the volume of the current music player. Offsets " +
                 "may also be used, for example: `+70` or `-25` will adjust your volume " +
                 "by 70 points louder or 25 points quieter than it was previously. The " +
                 "`mute` keyword may also be used to mute the player, though this is the " +
                 "same as setting the volume to zero.\n\n" +
                 "The maximum total volume a player may have is `250`, and the lowest is `0`.")]
        [Remarks("<amount>\n70\n+50\n-35\nmute")]
        [RequireUserPermission(GuildPermission.Connect)]
        [RequireBotPermission(GuildPermission.Connect)]
        [RequireContext(ContextType.Guild)]
        public async Task Command(string amount = null)
        {
            var node = ConfigProperties.LavaNode;
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            if (!node.HasPlayer(Context.Guild))
            {
                await SendBasicErrorEmbedAsync($"This server does not have an active music " +
                                               $"player. Please create one using the `{server.CommandPrefix}play` " +
                                               $"command.");
                return;
            }

            var player = node.GetPlayer(Context.Guild);
            if (string.IsNullOrWhiteSpace(amount))
            {
                await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Current Volume: `{player.Volume}`");
                return;
            }

            if (amount.Contains('+') && amount.Contains('-'))
            {
                await SendBasicErrorEmbedAsync($"You cannot have both a `+` and `-` volume adjuster at the same time.");
                return;
            }

            int limit = 250;
            VolumeAdjuster adjuster;

            if (amount.Contains('+'))
                adjuster = VolumeAdjuster.Increase;
            else if (amount.Contains('-'))
                adjuster = VolumeAdjuster.Decrease;
            else if (amount.ToLower().Equals("mute"))
                adjuster = VolumeAdjuster.Mute;
            else
                adjuster = VolumeAdjuster.Set;

            if (adjuster == VolumeAdjuster.Mute)
            {
                await player.UpdateVolumeAsync(0);
                await SendBasicSuccessEmbedAsync($"{Context.User.Mention} Successfully muted the music player.");
                return;
            }

            ushort volumeAdj = (ushort)amount.Split('+', '-').Last().AsInteger();

            switch (adjuster)
            {
                case VolumeAdjuster.Set:
                {
                    if (!(volumeAdj <= limit))
                    {
                        await SendBasicErrorEmbedAsync("The volume may not be set above 250.");
                        return;
                    }

                    await player.UpdateVolumeAsync(volumeAdj);
                    await SendBasicSuccessEmbedAsync($"Successfully set the volume to `{player.Volume}`");
                    break;
                }
                case VolumeAdjuster.Increase:
                {
                    if(!(player.Volume + volumeAdj <= 250))
                    {
                        await SendBasicSuccessEmbedAsync($"Successfully set the volume to max: `250`.");
                        await player.UpdateVolumeAsync(250);
                        return;
                    }

                    await player.UpdateVolumeAsync((ushort)(player.Volume + volumeAdj));
                    await SendBasicSuccessEmbedAsync($"Successfully adjusted the volume by `{amount}`. " +
                                                     $"The volume is now {player.Volume}.");
                    break;
                }
                case VolumeAdjuster.Decrease:
                {
                    if (player.Volume - volumeAdj < 0)
                    {
                        await SendBasicSuccessEmbedAsync("Successfully muted the player.");
                        await player.UpdateVolumeAsync(0);
                        return;
                    }
                    else
                    {
                        await SendBasicSuccessEmbedAsync($"Successfully reduced the volume by `{amount}`. The " +
                                                         $"current volume is now `{player.Volume - volumeAdj}`");
                    }
                    await player.UpdateVolumeAsync((ushort) (player.Volume - volumeAdj));
                    break;
                }
                case VolumeAdjuster.Mute:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum VolumeAdjuster
    {
        Increase,
        Decrease,
        Set,
        Mute
    }
}
