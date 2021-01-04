using System;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Humanizer.Localisation;
using Interactivity;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes.Enums;
using Kaguya.Discord.DiscordExtensions;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("redeem")]
    public class Redeem : KaguyaBase<Redeem>
    {
        private readonly ILogger<Redeem> _logger;
        private readonly PremiumKeyRepository _premiumKeyRepository;
        private readonly InteractivityService _interactivityService;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly KaguyaServerRepository _kaguyaServerRepository;

        public Redeem(ILogger<Redeem> logger, PremiumKeyRepository premiumKeyRepository, InteractivityService interactivityService,
            KaguyaUserRepository kaguyaUserRepository, KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
            _logger = logger;
            _premiumKeyRepository = premiumKeyRepository;
            _interactivityService = interactivityService;
            _kaguyaUserRepository = kaguyaUserRepository;
            _kaguyaServerRepository = kaguyaServerRepository;
        }

        [Command]
        [Summary("Used to redeem a [Kaguya Premium](" + Global.StoreUrl + ") key.")]
        [Remarks("<key>")]
        public async Task RedeemCommand(string key)
        {
            PremiumKey match = await _premiumKeyRepository.GetAsync(key);
            if (match == null)
            {
                var responseEmbed = GetBasicErrorEmbedBuilder("This premium key is invalid.").Build();
                _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, null, TimeSpan.FromSeconds(5), null, false, responseEmbed);

                return;
            }

            if (match.IsRedeemed)
            {
                var responseEmbed = GetBasicErrorEmbedBuilder("This key has already been redeemed.").Build();
                _interactivityService.DelayedSendMessageAndDeleteAsync(Context.Channel, null, TimeSpan.FromSeconds(5), null, false, responseEmbed);
                return;
            }

            var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            var server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);

            var additionalPoints = (int) (((double) 25000 / 30) * TimeSpan.FromSeconds(match.LengthInSeconds).TotalDays);
            var duration = new TimeSpan(0, 0, match.LengthInSeconds);
            
            // Modify PremiumKey
            match.Expiration = DateTime.Now.Add(duration);
            match.UserId = Context.User.Id;
            match.ServerId = Context.Guild.Id;
            
            // Modify User
            user.AdjustPoints(additionalPoints);
            user.TotalDaysPremium += (int)duration.TotalDays;
            user.TotalPremiumRedemptions++;
            user.PremiumExpiration = user.PremiumExpiration <= DateTime.Now || !user.PremiumExpiration.HasValue 
                ? match.Expiration 
                : user.PremiumExpiration.Value.Add(duration);
            
            // Modify Server
            server.PremiumExpiration = server.PremiumExpiration < DateTime.Now || !server.PremiumExpiration.HasValue 
                ? match.Expiration 
                : server.PremiumExpiration.Value.Add(duration);

            // Update in DB.
            await _kaguyaUserRepository.UpdateAsync(user);
            await _kaguyaServerRepository.UpdateAsync(server);
            await _premiumKeyRepository.UpdateAsync(match);

            TimeSpan userPremiumRemaining = user.PremiumExpiration.Value - DateTime.Now;
            TimeSpan serverPremiumRemaining = server.PremiumExpiration.Value - DateTime.Now;
                        
            var response = new KaguyaEmbedBuilder(Color.Gold)
                           .WithTitle("Kaguya Premium: Redemption Successful")
                           .WithDescription($"{Context.User.Mention} You have successfully redeemed a {Global.StoreNameWithLink} key " +
                                            $"with a duration of {match.HumanizedLength.AsBold()}.\n" +
                                            $"You have been awarded {additionalPoints.ToString("N0").AsBold()} points.")
                           .WithFooter(new EmbedFooterBuilder
                           {
                               Text = $"Premium Expiration: {Context.User.Username}: {userPremiumRemaining.Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day)} | " +
                                      $"{Context.Guild.Name}: {serverPremiumRemaining.Humanize(2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Day)}\n" +
                                      $"{Context.User.Username} has {user.TotalDaysPremium:N0} days premium across {user.TotalPremiumRedemptions:N0} redemptions."
                           })
                           .Build();

            await SendEmbedAsync(response);
        }
    }
}