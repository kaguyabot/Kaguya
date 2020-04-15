using System;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Threading.Tasks;
using Discord.Net;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Invite : KaguyaBase
    {
        [HelpCommand]
        [Command("Invite")]
        [Summary("DMs the user with a link to invite the bot to their own server, as " +
                 "well as a link to the Kaguya Support Discord server.")]
        [Remarks("")]
        public async Task InviteDM()
        {
            string devInviteUrl = "[[Kaguya Dev Invite]](https://discordapp.com/api/oauth2/authorize?client_id=367403886841036820&permissions=8&scope=bot)\n";
            const string inviteUrl = "[[Invite Kaguya to your server]](https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=469101694)\n";
            const string discordUrl = "[[Kaguya Support Discord]](https://discord.gg/aumCJhr)\n";

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
            catch (HttpException e)
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
