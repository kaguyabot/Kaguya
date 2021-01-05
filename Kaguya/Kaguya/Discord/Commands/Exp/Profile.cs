using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Exp
{
    [Module(CommandModule.Games)]
    [Group("profile")]
    [Alias("p")]
    public class Profile : KaguyaBase<Profile>
    {
        private readonly ILogger<Profile> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        public Profile(ILogger<Profile> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command]
        [Summary("Displays your Kaguya profile.")]
        public async Task ProfileCommand()
        {
            var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

            var percent = user.PercentToNextLevel;
            
            // TODO: Implement server exp.
            string title = $"Kaguya Profile for {Context.User}";
            var profileBuilder = new StringBuilder()
                                 .AppendLine($"Global Exp: {user.GlobalExp.ToString("N0").AsBold()} | Level: {user.GlobalExpLevel.ToString("N0").AsBold()} " +
                                             $"| ({(percent * 100):N2}% to {user.GlobalExpLevel + 1})")
                                 .AppendLine($"Fish Exp: {user.FishExp.ToString("N0").AsBold()} | Fish Level: {user.FishLevel.ToString("N0").AsBold()} " +
                                             $"| Fish Caught: IMPLEMENT")
                                 .AppendLine($"Points: {user.Points.ToString("N0").AsBold()}");

            var embed = new KaguyaEmbedBuilder(KaguyaColors.Teal)
                        .WithTitle(title)
                        .WithDescription(profileBuilder.ToString())
                        .Build();

            await SendEmbedAsync(embed);
        }
    }
}