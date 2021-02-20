using System;
using System.Threading.Tasks;
using Discord;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.PrimitiveExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal.Services
{
    public class ExperienceService
    {
        /// <summary>
        /// How much experience to add to users when they are eligible.
        /// </summary>
        public const int EXP_VALUE = 8;

        /// <summary>
        /// How many coins a user should receive upon earning experience.
        /// </summary>
        public const int COINS_VALUE = 2;
        /// <summary>
        /// The most frequent time period a user can concurrently earn experience.
        /// </summary>
        public static readonly TimeSpan SpamPreventionWindow = TimeSpan.FromMinutes(2);

        private readonly ILogger<ExperienceService> _logger;
        private readonly ITextChannel _textChannel;
        private readonly KaguyaUser _user;
        private readonly IUser _discordUser;
        private readonly ulong _serverId;
        private readonly ServerExperienceRepository _serverExperienceRepository;
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        public ExperienceService(ILogger<ExperienceService> logger, ITextChannel textChannel, KaguyaUser user, IUser discordUser, 
            ulong serverId, ServerExperienceRepository serverExperienceRepository, KaguyaUserRepository kaguyaUserRepository)
        {
            _logger = logger;
            _textChannel = textChannel;
            _user = user;
            _discordUser = discordUser;
            _serverId = serverId;
            _serverExperienceRepository = serverExperienceRepository;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        /// <summary>
        /// Adds server experience to users if they are eligible.
        /// </summary>
        /// <returns></returns>
        public async Task TryAddServerExperienceAsync()
        {
            if (!await CanReceiveServerExperienceAsync())
            {
                return;
            }
            
            ServerExperience match = await FetchExperienceAsync();
            int oldExp = match.Exp;
            int newExp = match.Exp + EXP_VALUE;
            
            await _serverExperienceRepository.AddAsync(_serverId, _user.UserId, EXP_VALUE);
            _logger.LogDebug($"(Server Exp) User {_user} has received {EXP_VALUE} EXP in server {_serverId}.");

            if (HasLeveledUp(oldExp, newExp))
            {
                _logger.LogInformation($"(Server Exp) User {_user} has leveled up! New level: {_user.GlobalExpLevel:N0}");
                int newLevel = CalculateLevel(newExp).ToFloor();
                await SendBasedOnPreferencesAsync(await GetLevelUpEmbedAsync(newLevel, true));
            }
        }

        public async Task TryAddGlobalExperienceAsync()
        {
            if (!CanReceiveGlobalExperience())
            {
                return;
            }

            int oldExp = _user.GlobalExp;
            int newExp = oldExp + EXP_VALUE;
            
            // Modify the user's exp and coins values
            _user.AdjustExperienceGlobal(EXP_VALUE);
            _user.AdjustCoins(COINS_VALUE);
            await _kaguyaUserRepository.UpdateAsync(_user);

            _logger.LogDebug($"(Global Exp) User {_user} has received {EXP_VALUE} exp and {COINS_VALUE} coins. New total: {_user.GlobalExp:N0} exp, {_user.Coins:N0} coins");
            
            if (HasLeveledUp(oldExp, newExp))
            {
                _logger.LogInformation($"(Global Exp) User {_user} has leveled up! New level: {_user.GlobalExpLevel:N0}");
                await SendBasedOnPreferencesAsync(await GetLevelUpEmbedAsync(_user.GlobalExpLevel, false));
            }
        }

        private async Task SendBasedOnPreferencesAsync(Embed embed)
        {
            ExpNotificationPreference notifType = _user.ExpNotificationType;
            IDMChannel dmChannel = await _discordUser.GetOrCreateDMChannelAsync();
            
            switch (notifType)
            {
                case ExpNotificationPreference.Both:
                    await SendToChannelAsync(embed);
                    await SendToDmAsync(embed, dmChannel);

                    break;
                case ExpNotificationPreference.Dm:
                    await SendToDmAsync(embed, dmChannel);

                    break;
                case ExpNotificationPreference.Disabled:
                    break;
                default: // If set to chat only.
                    await SendToChannelAsync(embed);

                    break;
            }
        }

        private async Task SendToChannelAsync(Embed embed)
        {
            await _textChannel.SendMessageAsync(embed: embed);
        }

        private async Task SendToDmAsync(Embed embed, IDMChannel channel)
        {
            try
            {
                await channel.SendMessageAsync(embed: embed);
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Failed to send level-up DM to user {channel.Recipient.Id}: {e.Message}.");
            }
        }
        
        private bool CanReceiveGlobalExperience()
        {
            return _user.LastGivenExp < DateTimeOffset.Now.Subtract(SpamPreventionWindow);
        }
        
        private async Task<bool> CanReceiveServerExperienceAsync()
        {
            ServerExperience serverExp = await FetchExperienceAsync();
                
            // Whether the user was given server EXP in the last 2 minutes.
            return serverExp.LastGivenExp < DateTimeOffset.Now.Subtract(SpamPreventionWindow);
        }

        private async Task<ServerExperience> FetchExperienceAsync()
        {
            return await _serverExperienceRepository.GetOrCreateAsync(_serverId, _user.UserId);
        }

        // TODO: Change to an actual image like before.
        /// <summary>
        /// Sends a level-up notification to the user.
        /// </summary>
        /// <param name="level">The level the user just reached.</param>
        /// <param name="server"></param>
        /// <returns></returns>
        private async Task<Embed> GetLevelUpEmbedAsync(int level, bool server)
        {
            Embed embed;
            int rank, total, exp;

            if (server)
            {
                rank = await _serverExperienceRepository.FetchRankAsync(_serverId, _user.UserId);
                total = await _serverExperienceRepository.GetCountAsync();
                exp = (await FetchExperienceAsync()).Exp;
                
                embed = new KaguyaEmbedBuilder(Color.Gold)
                {
                    Title = "Server Level Up!",
                    Description = $"{_discordUser.Mention} You've reached server level {level.ToString().AsBold()}!\n",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Server Rank: #{rank:N0} / {total:N0} | Server Exp: {exp:N0}"
                    }
                }.Build();
            }
            else
            {
                rank = await _kaguyaUserRepository.FetchExperienceRankAsync(_user.UserId);
                total = await _kaguyaUserRepository.GetCountAsync();
                exp = (await _kaguyaUserRepository.GetOrCreateAsync(_user.UserId)).GlobalExp; // todo: dont fetch again.
                
                embed = new KaguyaEmbedBuilder(Color.Gold)
                {
                    Title = "Global Level Up!",
                    Description = $"{_discordUser.Mention} You've reached level {level.ToString().AsBold()}!\n",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Global Rank: #{rank:N0} / {total:N0} | Global Exp: {exp:N0}"
                    }
                }.Build();
            }

            return embed;
        }

        public static bool HasLeveledUp(int oldExp, int newExp)
        {
            return CalculateLevel(oldExp).ToFloor() < CalculateLevel(newExp).ToFloor();
        }
        
        public static double CalculateLevel(int exp)
        {
            if (exp < 64)
                return 0;
	        
            return Math.Sqrt((exp / 8) - 8);
        }

        public static int CalculateExpFromLevel(double level)
        {
            return (int) (8 * Math.Pow(level, 2));
        }

        public static double CalculatePercentToNextLevel(double level, int exp)
        {
            int baseExp = CalculateExpFromLevel(level);
            int nextExp = CalculateExpFromLevel(level + 1);
            int difference = nextExp - baseExp;
            int remaining = nextExp - exp;

            return (difference - remaining) / (double)difference;
        }
    }
}