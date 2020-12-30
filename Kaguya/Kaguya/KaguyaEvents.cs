using System;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya
{
    public static class KaguyaEvents
    {
        public static event Func<PremiumKey, Task> OnPremiumKeyRedemption;
        public static void TriggerPremiumKeyRedemption(PremiumKey key) => OnPremiumKeyRedemption?.Invoke(key);
    }
}