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
    [Group("adjustpoints")]
    public class AdjustPoints : KaguyaBase<AdjustPoints>
    {
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        protected AdjustPoints(ILogger<AdjustPoints> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command]
        [Summary("Modifies the user's points by the specified amount.")]
        [Remarks("<user id> <points>")]
        public async Task CommandAdjustPoints(ulong userId, int points)
        {
            KaguyaUser user = await _kaguyaUserRepository.GetOrCreateAsync(userId);
            user.AdjustPoints(points);

            await _kaguyaUserRepository.UpdateAsync(user);

            string pointsString = $"+{points:N0}".AsBold();
            
            if (points < 0)
            {
                pointsString = points.ToString("N0").AsBold();
            }
            
            await SendBasicSuccessEmbedAsync($"Modified {userId.ToString().AsBold()}'s points by {pointsString}.\n\n" +
                                             $"They now have {user.Points.ToString("N0").AsBold()} points");
        }
    }
}