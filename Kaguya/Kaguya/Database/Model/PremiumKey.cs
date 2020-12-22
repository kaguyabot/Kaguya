using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Humanizer;
using Humanizer.Localisation;

namespace Kaguya.Database.Model
{
    public class PremiumKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Key { get; set; }
        public ulong KeyCreatorId { get; set; }
        public int LengthInSeconds { get; set; }
        public DateTime? Expiration { get; set; }
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }
        public string HumanizedDuration => new TimeSpan(0, 0, this.LengthInSeconds).Humanize(3, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day);

        public static string GenerateKey()
        {
            Random r = new Random();
            string possibleChars = "abcdefghijklmnopqrstuvwxyz1234567890!@#$%^&()_+";
            char[] chars = possibleChars.ToCharArray();
            List<char> finalSequence = new List<char>();

            for (int i = 0; i < 25; i++)
            {
                int index = r.Next(chars.Length);
                bool capitalized = index >= 0 && index <= 25 && index % 2 == 0;
                char toAdd = chars[index];
                if (capitalized)
                {
                    toAdd = Char.ToUpper(toAdd);
                }
                finalSequence.Add(toAdd);
            }

            return new string(finalSequence.ToArray());
        }
    }
}