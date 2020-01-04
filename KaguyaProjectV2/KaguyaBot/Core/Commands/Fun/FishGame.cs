using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class FishGame : InteractiveBase<ShardedCommandContext>
    {
        [FunCommand]
        [Command("Fish")]
        [Alias("f")]
        [Summary("Allows you to play the fishing game! Requires one bait per play. Bait may be purchased with " +
                 "the `buybait` command.\n\n" +
                 "Information:\n\n" +
                 "- You must have bait to fish. One bait costs 50 points " +
                 "(25% off for [Kaguya Supporters](https://the-kaguya-project.myshopify.com/)).\n" +
                 "- You may only fish once every 15 seconds (5 seconds for supporters).\n" +
                 "- Fish may be sold with the `sell` command!\n" +
                 "- View your fish collection with the `myfish` command!\n\n" +
                 "Happy fishing, and good luck catching the **Legendary `Big Kahuna`**!")]
        [Remarks("")]
        public async Task Command()
        {
            var user = await UserQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);

            if (user.FishBait < 1)
            {
                var baitEmbed = new KaguyaEmbedBuilder(EmbedColor.RED)
                {
                    Description = $"You are out of bait. Please buy more bait with the " +
                                  $"`{server.CommandPrefix}buybait` command!",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Bait costs {Fish.BAIT_COST} points ({Fish.SUPPORTER_BAIT_COST} for " +
                               $"active [Kaguya Supporters]({HelpfulObjects.KAGUYA_STORE_URL})."
                    }
                };
                await Context.Channel.SendEmbedAsync(baitEmbed);
                return;
            }

            if (user.LastFished >= DateTime.Now.AddSeconds(-15).ToOADate() && !user.IsSupporter ||
                user.LastFished >= DateTime.Now.AddSeconds(-5).ToOADate() && user.IsSupporter)
            {
                var ts = DateTime.FromOADate(user.LastFished) - DateTime.Now.AddSeconds(-15);

                if (user.IsSupporter)
                    ts += TimeSpan.FromSeconds(-10);

                var errorEmbed = new KaguyaEmbedBuilder(EmbedColor.RED)
                {
                    Description = $"Please wait `{ts.Humanize(minUnit: TimeUnit.Second)}` before fishing again."
                };

                await ReplyAndDeleteAsync("", false, errorEmbed.Build(), TimeSpan.FromSeconds(2.5));
                return;
            }
    
            int value;

            var embed = new KaguyaEmbedBuilder
            {
                Description = $"🎣 | {Context.User.Mention} "
            };

            Random r = new Random();
            double roll = r.NextDouble();
            int fishId = r.Next(int.MaxValue);

            while (await UtilityQueries.FishExistsAsync(fishId))
            {
                fishId = r.Next(int.MaxValue);
            }

            var fishType = GetFishType(roll);

            switch (fishType)
            {
                case FishType.SEAWEED:
                    value = 2;
                    embed.Description += $"Aw man, you caught `seaweed`. Better luck next time!";
                    embed.SetColor(EmbedColor.GRAY);
                    break;
                case FishType.PINFISH:
                    value = 15;
                    embed.Description += $"you caught a `pinfish`!";
                    embed.SetColor(EmbedColor.GRAY);
                    break;
                case FishType.SMALL_BASS:
                    value = 25;
                    embed.Description += $"you caught a `small bass`!";
                    embed.SetColor(EmbedColor.GREEN);
                    break;
                case FishType.SMALL_SALMON:
                    value = 25;
                    embed.Description += $"you caught a `small salmon`!";
                    embed.SetColor(EmbedColor.GREEN);
                    break;
                case FishType.CATFISH:
                    value = 75;
                    embed.Description += $"you caught a `catfish`!";
                    embed.SetColor(EmbedColor.GREEN);
                    break;
                case FishType.LARGE_BASS:
                    value = 150;
                    embed.Description += $"Wow, you caught a `large bass`!";
                    embed.SetColor(EmbedColor.LIGHT_BLUE);
                    break;
                case FishType.LARGE_SALMON:
                    value = 150;
                    embed.Description += $"Wow, you caught a `large salmon`!";
                    embed.SetColor(EmbedColor.LIGHT_BLUE);
                    break;
                case FishType.RED_DRUM:
                    value = 200;
                    embed.Description += $"Holy smokes, you caught a `red drum`!";
                    embed.SetColor(EmbedColor.RED);
                    break;
                case FishType.TRIGGERFISH:
                    value = 350;
                    embed.Description += $"Holy smokes, you caught a `triggerfish`!";
                    embed.SetColor(EmbedColor.LIGHT_PURPLE);
                    break;
                case FishType.GIANT_SEA_BASS:
                    value = 500;
                    embed.Description += $"No way, you caught a `giant sea bass`! Nice work!";
                    embed.SetColor(EmbedColor.LIGHT_PURPLE);
                    break;
                case FishType.SMALLTOOTH_SAWFISH:
                    value = 1000;
                    embed.Description += $"No way, you caught a `smalltooth sawfish`! Nice work!";
                    embed.SetColor(EmbedColor.LIGHT_PURPLE);
                    break;
                case FishType.DEVILS_HOLE_PUPFISH:
                    value = 2500;
                    embed.Description += $"I can't believe my eyes!! you caught a `devils hold pupfish`! You're crazy!";
                    embed.SetColor(EmbedColor.VIOLET);
                    break;
                case FishType.ORANTE_SLEEPER_RAY:
                    value = 5000;
                    embed.Description += $"Hot diggity dog, you caught an `orante sleeper ray`! This is unbelievable!";
                    embed.SetColor(EmbedColor.ORANGE);
                    break;
                case FishType.GIANT_SQUID:
                    value = 25000;
                    embed.Description += $"Well butter my buttcheeks and call me a biscuit, you caught the second " +
                                         $"rarest fish in the sea! It's a `giant squid`!! Congratulations!";
                    embed.SetColor(EmbedColor.ORANGE);
                    break;
                case FishType.BIG_KAHUNA:
                    value = 250000;
                    embed.Description += $"<a:siren:429784681316220939> NO WAY! You hit the jackpot " +
                                         $"and caught the **Legendary `BIG KAHUNA`**!!!! " +
                                         $"What an incredible moment this is! <a:siren:429784681316220939>";
                    embed.SetColor(EmbedColor.GOLD);
                    break;
                default:
                    value = 0;
                    embed.Description += $"Oh no, it took your bait! Better luck next time...";
                    embed.SetColor(EmbedColor.GRAY);
                    break;
            }

            user.FishBait -= 1;
            user.LastFished = DateTime.Now.ToOADate();

            var fish = new Fish
            {
                FishId = fishId,
                UserId = Context.User.Id,
                ServerId = Context.Guild.Id,
                TimeCaught = DateTime.Now.ToOADate(),
                FishType = fishType,
                FishString = fishType.ToString(),
                Value = value,
                Sold = false
            };

            await UserQueries.AddFish(fish);
            await UserQueries.UpdateUserAsync(user);

            if (fishType != FishType.BAIT_STOLEN)
            {
                var _ = await UserQueries.GetFishForUserAsync(fishType, user.Id);
                var fishCount = _.Count;
                var fishString = fishType.ToString().Replace("_", " ").ToLower();

                embed.Description += $"\n\nFish ID: `{fishId}`\n" +
                                     $"Fish Value: `{value:N0}` points.\n" +
                                     $"Bait Remaining: `{user.FishBait:N0}`\n\n" +
                                     $"You now have `{fishCount}` `{fishString}`";
            }
            else
            {
                embed.Description += $"\nBait Remaining: `{user.FishBait:N0}`";
            }

            embed.Footer = new EmbedFooterBuilder
            {
                Text = $"Use the {server.CommandPrefix}myfish command to view your fishing stats!\n" +
                       $"The {server.CommandPrefix}sellfish command may be used to sell your fish."
            };
            await ReplyAsync(embed: embed.Build());
        }

        private FishType GetFishType(double roll)
        {
            if (roll <= 0.0005)
                return FishType.BIG_KAHUNA;
            if (roll > 0.0005 && roll <= 0.0015)
                return FishType.GIANT_SQUID;
            if (roll > 0.0015 && roll <= 0.0030)
                return FishType.ORANTE_SLEEPER_RAY;
            if (roll > 0.0035 && roll <= 0.0050)
                return FishType.DEVILS_HOLE_PUPFISH;
            if (roll > 0.0050 && roll <= 0.01)
                return FishType.SMALLTOOTH_SAWFISH;
            if (roll > 0.01 && roll <= 0.03)
                return FishType.GIANT_SEA_BASS;
            if (roll > 0.03 && roll <= 0.05)
                return FishType.TRIGGERFISH;
            if (roll > 0.05 && roll <= 0.10)
                return FishType.RED_DRUM;
            if (roll > 0.10 && roll <= 0.17)
                return FishType.LARGE_SALMON;
            if (roll > 0.17 && roll <= 0.24)
                return FishType.LARGE_BASS;
            if (roll > 0.24 && roll <= 0.33)
                return FishType.CATFISH;
            if (roll > 0.33 && roll <= 0.43)
                return FishType.SMALL_SALMON;
            if (roll > 0.43 && roll <= 0.53)
                return FishType.SMALL_BASS;
            if (roll > 0.53 && roll <= 0.65)
                return FishType.PINFISH;
            if (roll > 0.65 && roll <= 0.75)
                return FishType.SEAWEED;
            return FishType.BAIT_STOLEN;
        }
    }
}
