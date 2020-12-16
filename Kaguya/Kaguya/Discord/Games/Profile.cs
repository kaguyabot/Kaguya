using System;
using System.Text;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Data;
using Kaguya.Discord.DiscordExtensions;

namespace Kaguya.Discord.Games
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
            var profile = new KaguyaUserProfile(user, _kaguyaUserRepository);

            var percent = profile.PercentToNextLevel;
            
            // TODO: Implement server exp.
            string title = $"Kaguya Profile for {Context.User}";
            var profileBuilder = new StringBuilder()
                                 .AppendLine($"Global Exp: {user.GlobalExp.ToString("N0").AsBold()} | Level: {profile.GlobalExpLevel.ToString("N0").AsBold()} " +
                                             $"| ({(percent * 100):N2}% to {profile.GlobalExpLevel + 1})")
                                 .AppendLine($"Fish Exp: {user.FishExp.ToString("N0").AsBold()} | Fish Level: {profile.FishLevel.ToString("N0").AsBold()} " +
                                             $"| Fish Caught: IMPLEMENT")
                                 .AppendLine($"Points: {user.Points.ToString("N0").AsBold()}");

            var embed = new KaguyaEmbedBuilder(Color.Teal)
                        .WithTitle(title)
                        .WithDescription(profileBuilder.ToString())
                        .Build();

            await SendEmbedAsync(embed);
        }
    }
}