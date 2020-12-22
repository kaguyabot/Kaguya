using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kaguya.Database.Model
{
    public class PremiumKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Key { get; set; }
        public ulong KeyCreatorId { get; set; }
        public long LengthInSeconds { get; set; }
        public DateTime? Expiration { get; set; }
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }

        public static string GenerateKey()
        {
            Random r = new Random();
            string possibleChars = "abcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()_+";
            char[] chars = possibleChars.ToCharArray();
            List<char> finalSequence = new List<char>();

            for (int i = 0; i < 25; i++)
            {
                int index = r.Next(chars.Length);
                finalSequence.Add(chars[index]);
            }

            return new string(finalSequence.ToArray());
        }
    }
}