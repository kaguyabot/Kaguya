﻿using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Kaguya.Internal.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Internal.Enums;
using Kaguya.Database.Repositories;
using Kaguya.External.Services.TopGg;
using Kaguya.Internal.Extensions.DiscordExtensions;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("vote")]
    [Alias("upvote", "uv")]
    [Summary("Displays a link to vote for Kaguya on top.gg. Voting online automatically " +
             "earns you rewards!")]
    public class Vote : KaguyaBase<Vote>
    {
        private readonly ILogger<Vote> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;

        public Vote(ILogger<Vote> logger, KaguyaUserRepository kaguyaUserRepository) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
        }

        [Command]
        [InheritMetadata(CommandMetadata.Summary)]
        public async Task UpvoteLinkCommandAsync()
        {
            const string URL = Global.TopGgUpvoteUrl;
            var user = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

            var topGgSb = new StringBuilder("top.gg".AsBoldUnderlined() + "\n");

            if (user.CanUpvote)
            {
                topGgSb.AppendLine("Available Now!".AsBlueCode(URL));
            }
            else
            {
                // They must have an upvote value if ineligible to vote.
                string cooldown = DateTime.Now.Subtract(user.LastUpvoted!.Value).HumanizeTraditionalReadable();
                topGgSb.AppendLine($"Cooldown: {cooldown.AsBold()}");
            }

            int coins = UpvoteNotifierService.COINS;
            int exp = UpvoteNotifierService.EXP;

            var embed = new KaguyaEmbedBuilder(KaguyaColors.LightYellow)
            {
                Title = "Vote for Kaguya",
                Description = topGgSb.ToString(),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Rewards",
                        Value = $"`{coins} coins`\n" +
                                $"`{exp} global exp`"
                    }
                }
            }.WithFooter("Earn 2x rewards on weekends!");

            await SendEmbedAsync(embed);
        }
    }
}