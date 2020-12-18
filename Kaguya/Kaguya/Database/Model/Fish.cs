﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;
using Kaguya.Services;

namespace Kaguya.Database.Model
{
    public class Fish
    {
        /// <summary>
        /// A auto-incrementing ID for this unique fish.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FishId { get; }
        /// <summary>
        /// The ID of the user who caught this fish.
        /// </summary>
        public ulong UserId { get; init; }
        /// <summary>
        /// The ID of the server in which this fish was caught in.
        /// </summary>
        public ulong ServerId { get; init; }
        /// <summary>
        /// The ID of the text channel in which this fish was caught in.
        /// </summary>
        public ulong ChannelId { get; init; }
        /// <summary>
        /// When this fish was caught.
        /// </summary>
        public DateTime TimeCaught { get; set; }
        /// <summary>
        /// How much fish exp the user earned.
        /// </summary>
        public int ExpValue { get; set; }
        
        /// <summary>
        /// How many points the user received for catching this fish.
        /// </summary>
        public int PointValue { get; set; }
        /// <summary>
        /// How many points the user spent to catch this fish.
        /// </summary>
        public int CostOfPlay { get; set; }
        /// <summary>
        /// The cost of this fish before taking user fishing experience modifications into account.
        /// </summary>
        public int BaseCost => FishService.GetFishValue(Rarity).fishPoints;

        /// <summary>
        /// The type of fish caught (name).
        /// </summary>
        public FishType FishType { get; set; }
        /// <summary>
        /// The rarity of the caught fish, ranging from Trash to Legendary.
        /// </summary>
        public FishRarity Rarity { get; set; }
        /// <summary>
        /// The written version of the <see cref="FishType"/>, formatted in "Title Case".
        /// </summary>
        public string FishTypeString => FishType.Humanize(LetterCasing.Title);
    }
}