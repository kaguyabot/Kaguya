using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Discord;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Kaguya.Internal.Services.Recurring;
using Microsoft.Extensions.Logging;
using Victoria;

namespace Kaguya.Internal.Events
{
    /// <summary>
    /// Most event implementations (outside of direct lambdas) will live here.
    /// </summary>
    public class EventImplementations
    {
        private readonly ILogger<EventImplementations> _logger;
        private readonly IAntiraidService _arService;
        private readonly DiscordShardedClient _client;
        private readonly LavaNode _lavaNode;

        public EventImplementations(ILogger<EventImplementations> logger, IAntiraidService arService,
            DiscordShardedClient client, LavaNode lavaNode)
        {
            _logger = logger;
            _arService = arService;
            _client = client;
            _lavaNode = lavaNode;
        }

        /// <summary>
        /// Logs a user join event inside of the Antiraid notification service.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task OnUserJoinedAsync(SocketGuildUser user)
        {
            var userId = user.Id;
            var serverId = user.Guild.Id;

            await _arService.TriggerAsync(serverId, userId);
        }

        /// <summary>
        /// Sends the initial Owner greeting direct message once Kaguya enters a new guild.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task SendOwnerDmAsync(SocketGuild guild)
        {
            SocketGuildUser owner = guild.Owner;
            var messageBuilder = new StringBuilder()
                                 .AppendLine($"Hello {owner.Username}, thanks for adding me to your server!")
                                 .AppendLine()
                                 .AppendLine("Here's a list of links and suggestions to help you get started.")
                                 .AppendLine()
                                 .AppendLine($"- [YouTube Tutorial]({Global.VideoTutorialUrl})")
                                 .AppendLine()
                                 .AppendLine($"- [Quick start guide]({Global.WikiQuickStartUrl})")
                                 .AppendLine($"- [Privacy statement]({Global.WikiPrivacyUrl})")
                                 .AppendLine($"- [Kaguya Support]({Global.SupportDiscordUrl})")
                                 .AppendLine($"- [Invite Kaguya]({Global.InviteUrl})")
                                 .AppendLine()
                                 .AppendLine($"- [Kaguya Premium Store]({Global.StoreUrl})")
                                 .AppendLine($"- [Kaguya Premium Benefits]({Global.WikiPremiumBenefitsUrl})")
                                 .AppendLine()
                                 .AppendLine($"- Support us by upvoting on [top.gg]({Global.TopGgUpvoteUrl})!");

            var embed = new KaguyaEmbedBuilder(Color.Gold)
            {
                Title = "Hey!",
                Description = messageBuilder.ToString(),
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = _client.CurrentUser.GetAvatarUrl(),
                    Name = "Kaguya Bot"
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "If you need any assistance at all, join our support Discord. We are happy to help!!"
                }
            };
            
            try
            {
                IDMChannel dmChannel = await owner.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(embed: embed.Build());
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, $"Failed to send owner {owner.Id} the owner greeting message.");
            }
        }

        public async Task UpvoteNotifierAsync(Upvote vote)
        {
            const int COINS = 750;
            const int EXP = 200;
            
            var user = _client.GetUser(vote.UserId);

            var voteEmbed = new KaguyaEmbedBuilder(KaguyaColors.Magenta)
            {
                Title = "Top.GG Vote Rewards",
                Description = "Thanks for upvoting on top.gg! You've been awarded:\n" +
                              $"- {COINS.ToString().AsBold()} coins\n" +
                              $"- {EXP.ToString().AsBold()} exp"
            }
            .WithFooter("You can vote again in 12 hours!")
            .Build();

            try
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(embed: voteEmbed);
            }
            catch (Exception)
            {
                //
            }
        }

        /// <summary>
        /// Disposes any active music player for a particular <see cref="SocketGuild"/>.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="join">Whether this method was triggered by a guild join event. Should be
        /// false if the opposite is true. Should be null if invoking from elsewhere within the program.
        /// </param>
        /// <returns></returns>
        public async Task DisposeMusicPlayerAsync(SocketGuild arg, bool? join)
        {
            if (_lavaNode.TryGetPlayer(arg, out var player))
            {
                try
                {
                    await _lavaNode.LeaveAsync(player.VoiceChannel);
                    await player.DisposeAsync();
                    _logger.LogInformation($"Guild {arg.Id} had an active music player. " +
                                           $"It has been properly disposed of.");
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        /// <summary>
        /// Disconnects the player from the voice channel the bot was disconnected from. This is to
        /// protect against lingering players from hanging around in a server and blocking new
        /// song queues. This can happen if the bot is force disconnected from a voice channel.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public async Task ProtectPlayerIntegrityOnDisconnectAsync(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (arg1.Id != _client.CurrentUser.Id || arg3.VoiceChannel != null)
            {
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(arg2.VoiceChannel ?? arg3.VoiceChannel);
            }
            catch (Exception)
            {
                //
            }
        }
    }
}