using System;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Owner_Only
{
    public class NotifyOfV2 : KaguyaBase
    {
        [OwnerCommand]
        [DangerousCommand]
        [Command("V2Notify")]
        [Summary("Notifies all guilds that Kaguya is connected to of the new V2 changes. " +
                 "This message is sent in the server's default chat channel.")]
        [Remarks("")]
        [RequireContext(ContextType.Guild)]
        public async Task Command()
        {
            await ReplyAsync($"{Context.User.Mention} Notifying {ConfigProperties.Client.Guilds.Count:N0} guilds...");
            string notifyStr = $"**Attention: There is an incoming message from my creator!**\n\n" +
                               $"`All users of the Kaguya Bot, a new update has just gone live: " +
                               $"Kaguya Version 2.0. Kaguya v2 is a complete rewrite from the ground up. " +
                               $"The changelog may be found here: https://github.com/stageosu/Kaguya/blob/v2.0/README.md \n\n" +
                               $"It is critical that you read this, as some commands have different names " +
                               $"or do different functions. I advise you to rebrowse the help command list " +
                               $"to see what has changed. Thank you for continuing to use the Kaguya Bot; " +
                               $"it is an amazing tool and just got so, so much better with this update!\n\n" +
                               $"Warmly, Stage - Founder and Lead Developer of the Kaguya Project.`";

            int i = 0;
            int j = ConfigProperties.Client.Guilds.Count;
            foreach (var guild in ConfigProperties.Client.Guilds)
            {
                i++;
                try
                {
                    await guild.DefaultChannel.SendMessageAsync(notifyStr);
                }
                catch (Exception)
                {
                    await ConsoleLogger.LogAsync(
                        $"I tried to notify a guild of V2 changes, but I encountered an exception.", LogLvl.DEBUG);
                }
                await ConsoleLogger.LogAsync($"{i}/{j} guilds notified of V2 changes.", LogLvl.WARN);
            }

            await ReplyAsync($"{Context.User.Mention} All guilds have now been notified.");
        }
    }
}
