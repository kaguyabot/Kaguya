using System;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Threading.Tasks;
using Discord.Net;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Invite : KaguyaBase
    {
        [ReferenceCommand]
        [Command("Invite")]
        [Summary("DMs the user with a link to invite the bot to their own server, as " +
                 "well as a link to the Kaguya Support Discord server.")]
        [Remarks("")]
        public async Task InviteDm()
        {
            string devInviteUrl = $"[[Kaguya Dev Invite]]({ConfigProperties.KAGUYA_DEV_INVITE_URL})\n";
            string inviteUrl = $"[[Invite Kaguya to your server]]({ConfigProperties.KAGUYA_INVITE_URL})\n";
            string discordUrl = $"[[Kaguya Support Discord]]({ConfigProperties.KAGUYA_SUPPORT_DISCORD_URL})\n";

            if (Context.User.Id != ConfigProperties.BotConfig.BotOwnerId)
                devInviteUrl = null;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Kaguya Invite Links",
                Description = $"{inviteUrl}{discordUrl}{devInviteUrl}"
            };

            try
            {
                await Context.User.SendMessageAsync(embed: embed.Build());
            }
            catch (HttpException)
            {
                await ConsoleLogger.LogAsync("Tried to DM a user the Kaguya invite links " +
                                             "but an HttpException was thrown.", LogLvl.WARN);
            }

            await ReplyAsync(embed: new KaguyaEmbedBuilder
                {
                    Description = "Links sent! Check your DM <:Kaguya:581581938884608001>"
                }
                .Build());
        }
    }
}