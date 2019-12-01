﻿using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB.Mapping;

namespace KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models
{
    [Table(Name = "supporterkeys")]
    public class SupporterKey
    {
        [Column(Name = "Key"), NotNull]
        public string Key { get; set; }
        [Column(Name = "LengthInDays"), NotNull]
        public int Length { get; set; }
        [Column(Name = "ActivatedByUser"), Nullable]
        public uint UserId { get; set; }
        /// <summary>
        /// If this key is tied to a user, the time (as OADate) when the key expires.
        /// </summary>
        [Column(Name = "expiration"), Nullable]
        public double Expiration { get; set; }
    }
}