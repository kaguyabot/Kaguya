using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Kaguya.Discord;
using Kaguya.Discord.DiscordExtensions;
using OsuSharp;
using OsuSharp.Oppai;

namespace Kaguya.External.Osu
{
    public class OsuRecent : OsuBase
    {
        private readonly OsuData _osuData;
        private readonly ICommandContext _ctx;

        private const char POINT_RIGHT = '▸';

        public OsuRecent(OsuData osuData, ICommandContext ctx) : base(osuData.Client)
        {
            _osuData = osuData;
            _ctx = ctx;
        }

        public async Task<Embed> GetMostRecentForUserAsync()
        {
            var osuUser = await _osuData.GetOsuUserAsync();
            IReadOnlyList<Score> userRecentPlays = await _osuData.Client.GetUserRecentsByUserIdAsync(osuUser.UserId, _osuData.GameMode);
            Score recentPlay = userRecentPlays[0];

            // Assuming userRecentPlays is already ordered by date.
            int consecutiveTries = 1;
            foreach (var play in userRecentPlays)
            {
                if (play.BeatmapId == recentPlay.BeatmapId)
                {
                    consecutiveTries++;
                }
                else
                {
                    break;
                }
            }
            if (recentPlay == null)
            {
                throw new OsuException("The user does not have any recent plays.");
            }

            Beatmap recentMapInfo = await recentPlay.GetBeatmapAsync();
            PerformanceData recentPpInfo = await recentPlay.GetPPAsync();

            string modString = recentPlay.Mods == Mode.None ? "" : $" +{recentPlay.Mods.Humanize()}"; // todo: Test humanize
            string title = $"{recentMapInfo.Title} [{recentMapInfo.Difficulty}]{modString} {recentPpInfo.Stars:N2}★";

            string gradeEmote = GetEmoteForRank(recentPlay.Rank);
            
            StringBuilder descSb = new StringBuilder();
            descSb.AppendLine($"{gradeEmote} {POINT_RIGHT} " + "Combo:".AsBold() + $" x{recentPlay.MaxCombo:N0}/{recentMapInfo.MaxCombo:N0} " +
                              $"{POINT_RIGHT} " + "Score:".AsBold() + $" {recentPlay.TotalScore:N0}");

            string humanizedTimeAgo = !recentPlay.Date.HasValue
                ? string.Empty
                : $" | Submitted " + (DateTime.UtcNow - recentPlay.Date.Value).HumanizeTraditionalReadable() + " ago.";


            var footer = new EmbedFooterBuilder
            {
                Text = $"Try #{consecutiveTries} - osu!Bancho{humanizedTimeAgo}"
            };
            
            return new KaguyaEmbedBuilder(Color.Teal)
                   .WithAuthor(_ctx.User)
                   .WithTitle(title)
                   .WithDescription(descSb.ToString())
                   .WithFooter(footer)
                   .Build();
        }
    }
}