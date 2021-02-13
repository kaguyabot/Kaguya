using System;
using System.Linq;
using System.Text;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Humanizer.Localisation;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Services;

namespace Kaguya.Discord.Commands.Games
{
    [Module(CommandModule.Games)]
    [Group("fish")]
    [Alias("f")]
    public class FishGame : KaguyaBase<FishGame>
    {
        private readonly ILogger<FishGame> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly FishRepository _fishRepository;
        private const int COINS = 75;
        private const int PREMIUM_COINS = 50;
        
        public FishGame(ILogger<FishGame> logger, KaguyaUserRepository kaguyaUserRepository, FishRepository fishRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
            _fishRepository = fishRepository;
        }

        [Command]
        [Summary("Allows you to play the fishing game! Each play costs 75 coins (50 if command " +
                 "user is a premium subscriber).")]
        public async Task FishCommand()
        {
            var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            int coinsUsed = user.IsPremium ? PREMIUM_COINS : COINS;
            TimeSpan cooldown = user.IsPremium ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(15);
            
            // TODO: Get user fish level bonuses and apply them here.
            if (user.Coins < coinsUsed)
            {
                await SendBasicErrorEmbedAsync("You do not have enough coins to play the fishing game.\n" +
                                          $"Coins: {user.Coins.ToString().AsBold()} ({coinsUsed - user.Coins} needed)");

                return;
            }

            if (user.LastFished > DateTime.Now - cooldown)
            {
	            await SendBasicErrorEmbedAsync($"Please wait " + (user.LastFished.Value - DateTime.Now.Subtract(cooldown))
	                                                             .Humanize(1, minUnit:  TimeUnit.Millisecond, maxUnit: TimeUnit.Second)
	                                                             .AsBold() + " before fishing again.");

	            return;
            }

            FishRarity rarity = FishService.SelectRandomRarity();
            FishType randomFish = FishService.SelectFish(rarity);

            (int coins, int exp) fishValue = FishService.GetFishValue(rarity);

            var fish = new Fish
            {
                UserId = Context.User.Id,
                ServerId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                TimeCaught = DateTime.Now,
                ExpValue = fishValue.exp,
                CoinValue = fishValue.coins,
                CostOfPlay = coinsUsed,
                BaseCost = FishService.GetFishValue(rarity).fishCoins,
                FishType = randomFish,
                Rarity = rarity,
            };

            string rarityString = rarity.Humanize(LetterCasing.Title).AsBold();
            string typeString = randomFish.Humanize(LetterCasing.Title);
            
            await _fishRepository.InsertAsync(fish);

            int netCoins = fish.CoinValue - coinsUsed;
            user.AdjustCoins(netCoins);
            user.AdjustFishExperience(fish.ExpValue);
            user.LastFished = DateTime.Now;
            await _kaguyaUserRepository.UpdateAsync(user);

            string prefix = rarity switch
            {
                FishRarity.Trash => "Aw man, you caught a",
                FishRarity.Common => "Wow, you caught a",
                FishRarity.Uncommon => "Nice! You caught a",
                FishRarity.Rare => "Holy smokes! You caught a",
                FishRarity.UltraRare => "Hot diggity dog!! You just caught a",
                FishRarity.Legendary => "WOW!! You hit the jackpot and caught the",
                _ => "You caught a"
            };

            // Grammar
            string start = typeString[0].ToString().ToLower();
            bool fishStartsWithVowel = start == "a" || start == "e" || start == "i" || start == "o" || start == "u";
            
            if (prefix.EndsWith("a", StringComparison.OrdinalIgnoreCase) && fishStartsWithVowel)
            {
                prefix += "n";
            }

            if (fish.FishType == FishType.BaitStolen)
            {
                prefix = "Aw man, you had your";
            }
            
            // End grammar
            StringBuilder descBuilder = new StringBuilder($"🎣 | {Context.User.Mention} {prefix} {typeString.AsBold()}!\n\n")
                                        .AppendLine($"Fish ID: {fish.FishId.ToString().AsBold()}")
                                        .AppendLine($"Rarity: {rarityString}")
                                        .AppendLine($"Market value: {fish.CoinValue.ToString("N0").AsBold()} coins")
                                        .AppendLine($"Experience gained: " + $"+{fish.ExpValue:N0}".AsBold() + " fishing exp")
                                        .AppendLine("Coins remaining: " + $"{user.Coins:N0}".AsBold());

            var allFish = await _fishRepository.GetAllForUserAsync(user.UserId);
            int allCaught = allFish.Count;
            int allCoins = allFish.Sum(x => x.CoinValue);
            
            string footer = $"Fish Level: {user.FishLevel:N0} | Fish Caught: {allCaught:N0} | Coins from Fishing: {allCoins:N0}";
            
            Color color = rarity switch
            {
                FishRarity.Trash => KaguyaColors.DarkGrey,
                FishRarity.Common => KaguyaColors.LighterGrey,
                FishRarity.Uncommon => KaguyaColors.Green,
                FishRarity.Rare => KaguyaColors.Blue,
                FishRarity.UltraRare => KaguyaColors.Purple,
                FishRarity.Legendary => KaguyaColors.Orange,
                _ => KaguyaColors.Green
            };

            Embed embed = new KaguyaEmbedBuilder(color)
                          .WithDescription(descBuilder.ToString())
                          .WithFooter(footer)
                          .Build();

            await SendEmbedAsync(embed);
        }
    }
}