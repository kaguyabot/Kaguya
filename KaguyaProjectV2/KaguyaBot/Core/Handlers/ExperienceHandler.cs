using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class ExperienceHandler
    {
        public static void AddEXP(User user)
        {
            // If the user can receive exp, give them between 5 and 8.
            if (CanGetExperience(user))
            {
                Random r = new Random();
                int exp = r.Next(5, 8);

                user.Experience += exp;
                user.LatestEXP = DateTime.Now.ToOADate();
                Users.UpdateUser(user);
                Console.WriteLine($"User {user.Id} has been given {exp} exp.");
            }
            else
            {
                Console.WriteLine($"User {user.Id} is not eligible to receive EXP.");
            }
        }

        private static bool CanGetExperience(User user)
        {
            if (DateTime.Now.AddSeconds(-120).ToOADate() >= user.LatestEXP) //2 minutes in OA date time.
            {
                return true;
            }
            return false;
        }
    }
}
