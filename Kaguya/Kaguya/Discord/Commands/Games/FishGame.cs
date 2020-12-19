﻿using System;
using System.Linq;
using System.Text;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Services;

namespace Kaguya.Discord.Commands.Games
{
    [Module(CommandModule.Games)]
    [Group("fish")]
    [Alias("f")]
    public class FishGame : KaguyaBase<FishGame>
    {
        private readonly ILogger<FishGame> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private const int POINTS = 75;
        private const int PREMIUM_POINTS = 50;
        
        public FishGame(ILogger<FishGame> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command]
        [Summary("Allows you to play the fishing game! Each play costs 75 points (50 if premium).")]
        public async Task FishCommand()
        {
            var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            int pointsUsed = user.IsPremium ? PREMIUM_POINTS : POINTS;
            TimeSpan cooldown = user.IsPremium ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(15);
            // TODO: Get user fish level bonuses and apply them here.
            if (user.Points < pointsUsed)
            {
                await SendBasicErrorEmbedAsync("You do not have enough points to play the fishing game.\n" +
                                          $"Points: {user.Points.ToString().AsBold()} ({pointsUsed - user.Points} needed)");

                return;
            }

            FishRarity rarity = FishService.SelectRandomRarity();
            FishType randomFish = FishService.SelectFish(rarity);

            (int points, int exp) fishValue = FishService.GetFishValue(rarity);

            var fish = new Fish
            {
                UserId = Context.User.Id,
                ServerId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                TimeCaught = DateTime.Now,
                ExpValue = fishValue.exp,
                PointValue = fishValue.points,
                CostOfPlay = pointsUsed,
                FishType = randomFish,
                Rarity = rarity
            };
            
            // TODO: Insert fish into database.

            int netPoints = fish.PointValue - pointsUsed;
            user.AdjustPoints(netPoints);
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
                FishRarity.Legendary => "WOW!! You hit the jackpot and caught the"
            };

            // Grammar
            string start = fish.FishTypeString[0].ToString().ToLower();
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
            StringBuilder descBuilder = new StringBuilder($"🎣 | {Context.User.Mention} {prefix} {fish.FishTypeString.AsBold()}!\n\n")
                                        .AppendLine($"Fish ID: {fish.FishId.ToString().AsBold()}")
                                        .AppendLine($"Rarity: {rarity.Humanize(LetterCasing.Title).AsBold()}")
                                        .AppendLine($"Market value: {fish.PointValue.ToString("N0").AsBold()} points")
                                        .AppendLine($"Experience gained: " + $"+{fish.ExpValue:N0}".AsBold() + " fishing exp")
                                        .AppendLine("Points remaining: " + $"{user.Points:N0}".AsBold());

            string footer = $"Fish Exp: {user.FishExp:N0} | Fish Caught: IMPLEMENT | Points from Fishing: IMPLEMENT";
            
            Color color = rarity switch
            {
                FishRarity.Trash => Color.DarkGrey,
                FishRarity.Common => Color.LighterGrey,
                FishRarity.Uncommon => Color.Green,
                FishRarity.Rare => Color.Blue,
                FishRarity.UltraRare => Color.Purple,
                FishRarity.Legendary => Color.Orange
            };

            Embed embed = new KaguyaEmbedBuilder(color)
                          .WithDescription(descBuilder.ToString())
                          .WithFooter(footer)
                          .Build();

            await SendEmbedAsync(embed);
        }
    }
}