using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.PrimitiveExtensions;
using Kaguya.Internal.Services;
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
        private readonly ServerExperienceRepository _serverExperienceRepository;
        private readonly FishRepository _fishRepository;
        private readonly CommonEmotes _commonEmotes;

        public Profile(ILogger<Profile> logger, KaguyaUserRepository kaguyaUserRepository,
            ServerExperienceRepository serverExperienceRepository, FishRepository fishRepository,
            CommonEmotes commonEmotes) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
            _serverExperienceRepository = serverExperienceRepository;
            _fishRepository = fishRepository;
            _commonEmotes = commonEmotes;
        }

        [Command]
        [Summary("Displays your Kaguya profile.")]
        public async Task ProfileCommandAsync()
        {
            KaguyaUser user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            ServerExperience serverExp = await _serverExperienceRepository.GetOrCreateAsync(Context.Guild.Id, user.UserId);

            int globalExpRank = await _kaguyaUserRepository.FetchExperienceRankAsync(user.UserId);
            int globalUserCount = await _kaguyaUserRepository.GetCountAsync();
            
            int serverExpRank = await _serverExperienceRepository.FetchRankAsync(Context.Guild.Id, user.UserId);
            int serverExpCount = await _serverExperienceRepository.GetAllCountAsync(Context.Guild.Id);
            int serverExpLevel = ExperienceService.CalculateLevel(serverExp.Exp).ToFloor();
            
            int fishCount = await _fishRepository.CountAllNonTrashAsync(user.UserId);

            double percent = user.PercentToNextLevel;

            IEmote diamondsEmote = _commonEmotes.KaguyaDiamondsAnimated;
            
            string title = $"Kaguya Profile";
            StringBuilder profileBuilder = new StringBuilder()
                                           .AppendLine($"Global Exp: {user.GlobalExp.ToString("N0").AsBold()} | Level: {user.GlobalExpLevel.ToString("N0").AsBold()} " +
                                                       $"| ({percent:N2}% to {user.GlobalExpLevel + 1})")
                                           .AppendLine($"Server Exp: {serverExp.Exp.ToString("N0").AsBold()} | Server Level: {serverExpLevel.ToString("N0").AsBold()}")
                                           .AppendLine($"Fish Exp: {user.FishExp.ToString("N0").AsBold()} | Fish Level: {user.FishLevel.ToString("N0").AsBold()} " +
                                                       $"| Fish Caught: {fishCount.ToString("N0").AsBold()}")
                                           .AppendLine($"Global Rank: #{globalExpRank.ToString("N0").AsBold()} of {globalUserCount:N0} | " +
                                                       $"Server Rank: #{serverExpRank.ToString("N0").AsBold()} of {serverExpCount:N0}")
                                           .AppendLine($"Coins: {user.Coins.ToString("N0").AsBold()}");

            if (user.IsPremium)
            {
                profileBuilder.AppendLine($"{diamondsEmote} Thanks for being a Kaguya Premium subscriber! {diamondsEmote}".AsItalics());
            }

            Embed embed = new KaguyaEmbedBuilder(KaguyaColors.Blue)
                          .WithTitle(title)
                          .WithDescription(profileBuilder.ToString())
                          .WithAuthor(new EmbedAuthorBuilder
                          {
                              IconUrl = Context.User.GetAvatarUrl(),
                              Name = Context.User.Username
                          })
                          .Build();

            await SendEmbedAsync(embed);
        }
    }
}