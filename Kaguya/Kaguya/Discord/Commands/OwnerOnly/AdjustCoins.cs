using System.Threading.Tasks;
using Discord.Commands;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.OwnerOnly
{
    [Restriction(ModuleRestriction.OwnerOnly)]
    [Module(CommandModule.OwnerOnly)]
    [Group("adjustcoins")]
    public class AdjustCoins : KaguyaBase<AdjustCoins>
    {
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        protected AdjustCoins(ILogger<AdjustCoins> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command]
        [Summary("Modifies the user's coins by the specified amount.")]
        [Remarks("<user id> <coins>")]
        public async Task CommandAdjustCoins(ulong userId, int coins)
        {
            KaguyaUser user = await _kaguyaUserRepository.GetOrCreateAsync(userId);
            user.AdjustCoins(coins);

            await _kaguyaUserRepository.UpdateAsync(user);

            string coinsString = $"+{coins:N0}".AsBold();
            
            if (coins < 0)
            {
                coinsString = coins.ToString("N0").AsBold();
            }
            
            await SendBasicSuccessEmbedAsync($"Modified {userId.ToString().AsBold()}'s coins by {coinsString}.\n\n" +
                                             $"They now have {user.Coins.ToString("N0").AsBold()} coins");
        }
    }
}