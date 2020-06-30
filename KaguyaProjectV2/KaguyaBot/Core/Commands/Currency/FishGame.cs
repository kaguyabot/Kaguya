using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Configurations.Models;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class FishGame : KaguyaBase
    {
        [CurrencyCommand]
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
            const int FISHING_COOLDOWN = 15;
            const int FISHING_COOLDOWN_PREMIUM = 5;
            
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            
            if (user.FishBait < 1)
            {
                var baitEmbed = new KaguyaEmbedBuilder(EmbedColor.RED)
                {
                    Description = $"You are out of bait. Please buy more bait with the " +
                                  $"`{server.CommandPrefix}buybait` command!",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"One bait will cost you {user.FishbaitCost():N0} points."
                    }
                };
                await SendEmbedAsync(baitEmbed);
                return;
            }
            
            bool premium = await user.IsPremiumAsync();
            
            if (user.LastFished >= DateTime.Now.AddSeconds(-FISHING_COOLDOWN).ToOADate() && !premium ||
                user.LastFished >= DateTime.Now.AddSeconds(-FISHING_COOLDOWN_PREMIUM).ToOADate() && premium)
            {
                var ts = DateTime.FromOADate(user.LastFished) - DateTime.Now.AddSeconds(-15);

                if (premium)
                    ts -= TimeSpan.FromSeconds(10);

                var errorEmbed = new KaguyaEmbedBuilder(EmbedColor.RED)
                {
                    Description = $"Please wait `{ts.Humanize(minUnit: TimeUnit.Second)}` before fishing again."
                };

                await ReplyAndDeleteAsync("", false, errorEmbed.Build(), TimeSpan.FromSeconds(3));
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
            int fishExp;

            while (await DatabaseQueries.ItemExists<Fish>(x => x.FishId == fishId))
            {
                fishId = r.Next(int.MaxValue);
            }

            var bonuses = new FishHandler.FishLevelBonuses(user.FishExp);
            roll *= 1 - (bonuses.BonusLuckPercent / 100);

            if (premium || server.IsPremium)
                roll *= 0.95;
            
            var fishType = GetFishType(roll);

            switch (fishType)
            {
                case FishType.SEAWEED:
                    value = 2;
                    fishExp = 0;
                    embed.Description += $"Aw man, you caught `seaweed`. Better luck next time!";
                    embed.SetColor(EmbedColor.GRAY);
                    break;
                case FishType.PINFISH:
                    value = 15;
                    fishExp = r.Next(1, 3);
                    embed.Description += $"you caught a `pinfish`!";
                    embed.SetColor(EmbedColor.GRAY);
                    break;
                case FishType.SMALL_BASS:
                    value = 25;
                    fishExp = r.Next(2, 6);
                    embed.Description += $"you caught a `small bass`!";
                    embed.SetColor(EmbedColor.GREEN);
                    break;
                case FishType.SMALL_SALMON:
                    value = 25;
                    fishExp = r.Next(2, 6);
                    embed.Description += $"you caught a `small salmon`!";
                    embed.SetColor(EmbedColor.GREEN);
                    break;
                case FishType.CATFISH:
                    value = 75;
                    fishExp = r.Next(5, 9);
                    embed.Description += $"you caught a `catfish`!";
                    embed.SetColor(EmbedColor.GREEN);
                    break;
                case FishType.LARGE_BASS:
                    value = 150;
                    fishExp = r.Next(7, 11);
                    embed.Description += $"Wow, you caught a `large bass`!";
                    embed.SetColor(EmbedColor.LIGHT_BLUE);
                    break;
                case FishType.LARGE_SALMON:
                    value = 150;
                    fishExp = r.Next(7, 11);
                    embed.Description += $"Wow, you caught a `large salmon`!";
                    embed.SetColor(EmbedColor.LIGHT_BLUE);
                    break;
                case FishType.RED_DRUM:
                    value = 200;
                    fishExp = r.Next(7, 20);
                    embed.Description += $"Holy smokes, you caught a `red drum`!";
                    embed.SetColor(EmbedColor.RED);
                    break;
                case FishType.TRIGGERFISH:
                    value = 350;
                    fishExp = r.Next(11, 30);
                    embed.Description += $"Holy smokes, you caught a `triggerfish`!";
                    embed.SetColor(EmbedColor.LIGHT_PURPLE);
                    break;
                case FishType.GIANT_SEA_BASS:
                    value = 500;
                    fishExp = r.Next(18, 36);
                    embed.Description += $"No way, you caught a `giant sea bass`! Nice work!";
                    embed.SetColor(EmbedColor.LIGHT_PURPLE);
                    break;
                case FishType.SMALLTOOTH_SAWFISH:
                    value = 1000;
                    fishExp = r.Next(29, 42);
                    embed.Description += $"No way, you caught a `smalltooth sawfish`! Nice work!";
                    embed.SetColor(EmbedColor.LIGHT_PURPLE);
                    break;
                case FishType.DEVILS_HOLE_PUPFISH:
                    value = 2500;
                    fishExp = r.Next(40, 95);
                    embed.Description += $"I can't believe my eyes!! you caught a `devils hold pupfish`! You're crazy!";
                    embed.SetColor(EmbedColor.VIOLET);
                    break;
                case FishType.ORANTE_SLEEPER_RAY:
                    value = 5000;
                    fishExp = r.Next(75, 325);
                    embed.Description += $"Hot diggity dog, you caught an `orante sleeper ray`! This is unbelievable!";
                    embed.SetColor(EmbedColor.ORANGE);
                    break;
                case FishType.GIANT_SQUID:
                    value = 20000;
                    fishExp = r.Next(400, 900);
                    embed.Description += $"Well butter my buttcheeks and call me a biscuit, you caught the second " +
                                         $"rarest fish in the sea! It's a `giant squid`!! Congratulations!";
                    embed.SetColor(EmbedColor.ORANGE);
                    break;
                case FishType.BIG_KAHUNA:
                    value = 50000;
                    fishExp = r.Next(1250, 4500);
                    embed.Description += $"<a:siren:429784681316220939> NO WAY! You hit the jackpot " +
                                         $"and caught the **Legendary `BIG KAHUNA`**!!!! " +
                                         $"What an incredible moment this is! <a:siren:429784681316220939>";
                    embed.SetColor(EmbedColor.GOLD);
                    break;
                default:
                    value = 0;
                    fishExp = 0;
                    embed.Description += $"Oh no, it took your bait! Better luck next time...";
                    embed.SetColor(EmbedColor.GRAY);
                    break;
            }

            user.FishExp += fishExp;
            user.FishBait -= 1;
            user.LastFished = DateTime.Now.ToOADate();

            value = (int)(value * (1 + bonuses.BonusFishValuePercent / 100));
            var fish = new Fish
            {
                FishId = fishId,
                UserId = Context.User.Id,
                ServerId = Context.Guild.Id,
                TimeCaught = DateTime.Now.ToOADate(),
                FishType = fishType,
                FishString = fishType.ToString(),
                Value = value,
                Exp = fishExp,
                Sold = false
            };

            await DatabaseQueries.InsertAsync(fish);
            await DatabaseQueries.UpdateAsync(user);

            await FishEvent.Trigger(user, fish, Context); // Triggers the fish EXP service.

            if (fishType != FishType.BAIT_STOLEN)
            {
                var existingFish = (await DatabaseQueries.GetFishForUserMatchingTypeAsync(fishType, user.UserId)).ToList();
                var fishCount = existingFish.Count(x => !x.Sold);
                var fishString = fishType.ToString().Replace("_", " ").ToLower();

                embed.Description += $"\n\nFish ID: `{fishId}`\n" +
                                     $"Fish Value (excluding tax): `{value:N0}` points.\n" +
                                     $"Fishing Exp Earned: `{fishExp:N0} exp`\n" +
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
            // Fish Embed
            await ReplyAsync(embed: embed.Build());
        }

        private FishType GetFishType(double roll)
        {
            if (roll <= 0.0005) // 1 in 2000 chance. o_o
                return FishType.BIG_KAHUNA;
            if (roll > 0.0005 && roll <= 0.0015)
                return FishType.GIANT_SQUID;
            if (roll > 0.0015 && roll <= 0.0035)
                return FishType.ORANTE_SLEEPER_RAY;
            if (roll > 0.0035 && roll <= 0.0070)
                return FishType.DEVILS_HOLE_PUPFISH;
            if (roll > 0.01 && roll <= 0.025)
                return FishType.SMALLTOOTH_SAWFISH;
            if (roll > 0.025 && roll <= 0.05)
                return FishType.GIANT_SEA_BASS;
            if (roll > 0.05 && roll <= 0.085)
                return FishType.TRIGGERFISH;
            if (roll > 0.085 && roll <= 0.13)
                return FishType.RED_DRUM;
            if (roll > 0.13 && roll <= 0.20)
                return FishType.LARGE_SALMON;
            if (roll > 0.20 && roll <= 0.27)
                return FishType.LARGE_BASS;
            if (roll > 0.27 && roll <= 0.36)
                return FishType.CATFISH;
            if (roll > 0.36 && roll <= 0.46)
                return FishType.SMALL_SALMON;
            if (roll > 0.46 && roll <= 0.56)
                return FishType.SMALL_BASS;
            if (roll > 0.56 && roll <= 0.68)
                return FishType.PINFISH;
            if (roll > 0.68 && roll <= 0.78)
                return FishType.SEAWEED;
            return FishType.BAIT_STOLEN;
        }
    }
}
