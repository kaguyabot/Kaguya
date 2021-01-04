using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Kaguya.Database.Repositories;
using Kaguya.Discord.Attributes;
using Kaguya.Discord.Attributes.Enums;
using Kaguya.Discord.DiscordExtensions;
using Microsoft.Extensions.Logging;

namespace Kaguya.Discord.Commands.Exp
{
    [Module(CommandModule.Exp)]
    [Group("rep")]
    public class Rep : KaguyaBase<Rep>
    {
        private readonly ILogger<Rep> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly RepRepository _repRepository;

        public Rep(ILogger<Rep> logger, KaguyaUserRepository kaguyaUserRepository, RepRepository repRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
            _repRepository = repRepository;
        }

        [Command]
        [Summary("Allows you to give rep to another user. Limit 1 per 24 hours. The user must be in the " +
                 "Discord server this command is used from. An optional reason may be " +
                 "passed through at the end. Use without the user parameter to view your own rep.")]
        [Remarks("[user] [reason]")]
        public async Task RepCommand(SocketGuildUser user, [Remainder] string reason = "No reason provided.")
        {
            if (user.IsEqual(Context.User))
            {
                await SendBasicErrorEmbedAsync("You cannot give rep to yourself.");
            
                return;
            }
            
            var curUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            var nextUser = await _kaguyaUserRepository.GetOrCreateAsync(user.Id);

            if (!curUser.CanGiveRep)
            {
                var difference = curUser.LastGivenRep - DateTime.Now.AddHours(-24);
                await SendBasicErrorEmbedAsync($"Sorry, you need to wait " + difference.Value.Humanize(2).AsBold() + 
                                               " before giving rep again.");
            }
            else
            {
                var rep = new Database.Model.Rep
                {
                    UserId = nextUser.UserId,
                    GivenBy = curUser.UserId,
                    TimeGiven = DateTime.Now,
                    Reason = reason
                };
                
                curUser.LastGivenRep = DateTime.Now;

                await _kaguyaUserRepository.UpdateAsync(curUser);
                await _kaguyaUserRepository.UpdateAsync(nextUser);
                await _repRepository.InsertAsync(rep);

                await SendBasicSuccessEmbedAsync($"Gave +1 rep to {user.Mention}!");
            }
        }
        
        [Command]
        public async Task RepCommand()
        {
            var curUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            int repNum = await _repRepository.GetCountRepForUserAsync(curUser.UserId);

            var recentMatch = await _repRepository.GetMostRecentForUserAsync(curUser.UserId);
            bool showFooter = recentMatch != null;
            
            var embed = new KaguyaEmbedBuilder(Color.Green)
            {
                Description = $"{Context.User.Mention} you have " + repNum.ToString().AsBold() + " rep."
            };
            
            if (showFooter)
            {
                var guildRecentMatch = Context.Guild.GetUser(recentMatch.GivenBy);

                string byText = guildRecentMatch != null ? guildRecentMatch.ToString() : recentMatch.UserId.ToString();

                TimeSpan lastRepped = DateTime.Now - recentMatch.TimeGiven;
                embed.Footer = new EmbedFooterBuilder
                {
                    Text = $"Last given rep {lastRepped.Humanize()} ago by {byText}"
                };
            }

            await SendEmbedAsync(embed);
        }
    }
}