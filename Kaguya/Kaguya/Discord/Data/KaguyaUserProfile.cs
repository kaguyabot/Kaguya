using System;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;

namespace Kaguya.Discord.Data
{
    public class KaguyaUserProfile
    {
        private readonly KaguyaUser _user;
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        public KaguyaUserProfile(KaguyaUser user, KaguyaUserRepository kaguyaUserRepository)
        {
            _user = user;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        public int GlobalExpLevel => ToFloor(ExactGlobalExpLevel);

        /// <summary>
        /// The user's level as returned by the experience formula.
        /// </summary>
        public double ExactGlobalExpLevel => CalculateLevel(_user.GlobalExp);

        public int FishLevel => ToFloor(ExactFishLevel);

        public double ExactFishLevel => CalculateLevel(_user.FishExp);

        public int ExpToNextGlobalLevel => CalculateExpFromLevel(GlobalExpLevel + 1) - _user.GlobalExp;
        public double PercentToNextLevel => CalculatePercentToNextLevel();

        private static double CalculateLevel(int exp)
        {
	        if (exp < 64)
		        return 0;
	        
            return Math.Sqrt((exp / 8) - 8);
        }

        private static int CalculateExpFromLevel(double level)
        {
			return (int) (8 * Math.Pow(level, 2));
        }

        private double CalculatePercentToNextLevel()
        {
            int baseExp = CalculateExpFromLevel(GlobalExpLevel);
            int nextExp = CalculateExpFromLevel(GlobalExpLevel + 1);
            int difference = nextExp - baseExp;
            int remaining = nextExp - _user.GlobalExp;

            return (difference - remaining) / (double)difference;
        }

        private static int ToFloor(double d) => (int)Math.Floor(d);
    }
}