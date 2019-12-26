using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Help
{
    public class Invite : ModuleBase<ShardedCommandContext>
    {
        [HelpCommand]
        [Command("Invite")]
        [Summary("DMs the user with a link to invite the bot to their own server, as " +
                 "well as a link to the Kaguya Support Discord server.")]
        [Remarks("")]
        public async Task InviteDM()
        {
            string devInviteUrl = "[[Kaguya Dev Invite]](https://discordapp.com/api/oauth2/authorize?client_id=367403886841036820&permissions=8&scope=bot)\n";
            const string inviteUrl = "[[Invite Kaguya to your server]](https://discordapp.com/oauth2/authorize?client_id=538910393918160916&scope=bot&permissions=8)\n";
            const string discordUrl = "[[Kaguya Support Discord]](https://discord.gg/aumCJhr)\n";

            if (Context.User.Id != ConfigProperties.botConfig.BotOwnerId)
                devInviteUrl = null;

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Kaguya Invite Links",
                Description = $"{inviteUrl}{discordUrl}{devInviteUrl}"
            };

            await Context.User.SendMessageAsync(embed: embed.Build());

            await ReplyAsync(embed: new KaguyaEmbedBuilder
            {
                Description = "Links sent! Check your DM <:Kaguya:581581938884608001>"
            }
            .Build());
        }
    }
}
