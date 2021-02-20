using System;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Humanizer.Localisation;
using Interactivity;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Extensions.DiscordExtensions;

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
        [Summary("Used to redeem a [Kaguya Premium](" + Global.StoreUrl + ") key.\n\n" +
                 "Multiple keys can be pasted in, separated by a space, up to a maximum of 5 at a time.")]
        [Remarks("<key>")]
        public async Task RedeemCommand(params string[] keys)
        {
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch (Exception)
            {
                //
            }
            
            if (keys.Length > 5)
            {
                await SendBasicErrorEmbedAsync("You are trying to redeem too many items at once. Please " +
                                               "paste in up to 5 keys.");

                return;
            }
            
            foreach (var key in keys)
            {
                PremiumKey match = await _premiumKeyRepository.GetKeyAsync(key);
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

                var additionalCoins = (int) (((double) 25000 / 30) * TimeSpan.FromSeconds(match.LengthInSeconds).TotalDays);
                var duration = new TimeSpan(0, 0, match.LengthInSeconds);
                
                // Modify PremiumKey
                match.Expiration = DateTimeOffset.Now.Add(duration);
                match.UserId = Context.User.Id;
                match.ServerId = Context.Guild.Id;
                
                // Modify User
                user.AdjustCoins(additionalCoins);
                user.TotalDaysPremium += (int)duration.TotalDays;
                user.TotalPremiumRedemptions++;
                user.PremiumExpiration = user.PremiumExpiration <= DateTimeOffset.Now || !user.PremiumExpiration.HasValue 
                    ? match.Expiration 
                    : user.PremiumExpiration.Value.Add(duration);
                
                // Modify Server
                server.PremiumExpiration = server.PremiumExpiration < DateTimeOffset.Now || !server.PremiumExpiration.HasValue 
                    ? match.Expiration 
                    : server.PremiumExpiration.Value.Add(duration);

                // Update in DB.
                await _kaguyaUserRepository.UpdateAsync(user);
                await _kaguyaServerRepository.UpdateAsync(server);
                await _premiumKeyRepository.UpdateAsync(match);

                TimeSpan userPremiumRemaining = user.PremiumExpiration.Value - DateTimeOffset.Now;
                TimeSpan serverPremiumRemaining = server.PremiumExpiration.Value - DateTimeOffset.Now;
                
                _logger.LogInformation($"User {Context.User.Id} has redeemed a Kaguya Premium key in guild {Context.Guild.Id}: duration = {match.HumanizedLength}.");
                
                var response = new KaguyaEmbedBuilder(KaguyaColors.Gold)
                               .WithTitle("Kaguya Premium: Redemption Successful")
                               .WithDescription($"{Context.User.Mention} You have successfully redeemed a {Global.StoreLink} key " +
                                                $"with a duration of {match.HumanizedLength.AsBold()}.\n" +
                                                $"You have been awarded {additionalCoins.ToString("N0").AsBold()} coins.")
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
}