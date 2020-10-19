using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.EXP
{
    public class SetExpPreferences : KaguyaBase
    {
        [ExpCommand]
        [Command("ExpPreferences")]
        [Alias("xpp", "expp")]
        [Summary("Set your preferred method of being notified that you've leveled up. " +
                 "Users may choose to have `Server`, `Global`, `Both`, or `None` level-ups be " +
                 "announced to them via `Chat`, `DM`, `Both`, or `Disabled`.\n\n" +
                 "It is possible to configure, for example, global level-ups be sent to you in chat, " +
                 "but server level-ups be sent to you via DM. You may also disable specific level-up " +
                 "notifications for your account if you don't want to see a level-up notification, " +
                 "this could be both types of notifications as well.\n\n" +
                 "The default configuration is `both chat`. If you ever wish to reset your preferences, " +
                 "you will have to execute this command followed by `both chat`.\n\n" +
                 "Use without any parameters to view your current configuration.")]
        [Remarks("<level-up type> <route>\nServer DM")]
        [RequireContext(ContextType.Guild)]
        public async Task Command(string type = null, string channel = null)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (type == null && channel == null)
            {
                ExpType curChatType = user.ExpChatNotificationType;
                ExpType curDmType = user.ExpDmNotificationType;

                string userPref = $"{Context.User.Mention}, here are your current EXP level-up notification preferences:\n\n" +
                                  $"Chat reply: `{curChatType.Humanize(LetterCasing.Title)}`\n" +
                                  $"DM: `{curDmType.Humanize(LetterCasing.Title)}`";

                await SendBasicSuccessEmbedAsync(userPref);

                return;
            }

            ExpType expType = GetExpType(type);
            ExpChannel expChannel = GetChannelPref(channel);

            switch (expChannel)
            {
                case ExpChannel.CHAT:
                    user.ExpChatNotificationTypeNum = (int) expType;

                    break;
                case ExpChannel.DM:
                    user.ExpDmNotificationTypeNum = (int) expType;

                    break;
                case ExpChannel.BOTH:
                    user.ExpChatNotificationTypeNum = (int) expType;
                    user.ExpDmNotificationTypeNum = (int) expType;

                    break;
                case ExpChannel.DISABLED:
                    user.ExpChatNotificationTypeNum = (int) ExpType.NONE;
                    user.ExpDmNotificationTypeNum = (int) ExpType.NONE;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await DatabaseQueries.UpdateAsync(user);
            await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully updated your " +
                                                             $"EXP notification preferences.\n" +
                                                             $"New configuration:\n\n" +
                                                             $"Chat reply: `{user.ExpChatNotificationType.Humanize(LetterCasing.Title)}`\n" +
                                                             $"DM: `{user.ExpDmNotificationType.Humanize(LetterCasing.Title)}`");
        }

        private ExpChannel GetChannelPref(string channel) => channel.ToLower() switch
        {
            "chat" => ExpChannel.CHAT,
            "dm" => ExpChannel.DM,
            "both" => ExpChannel.BOTH,
            "disabled" => ExpChannel.DISABLED,
            _ => throw new ArgumentOutOfRangeException("The route parameter must be set " +
                                                       "to either `chat`, `dm`, `both`, or `disabled`.")
        };

        private ExpType GetExpType(string type) => type.ToLower() switch
        {
            "global" => ExpType.GLOBAL,
            "server" => ExpType.SERVER,
            "both" => ExpType.BOTH,
            "none" => ExpType.NONE,
            _ => throw new ArgumentOutOfRangeException("The level-up type parameter must be either " +
                                                       "`server`, `global`, `both`, or `none`.")
        };
    }

    public enum ExpType
    {
        GLOBAL,
        SERVER,
        BOTH,
        NONE
    }

    public enum ExpChannel
    {
        CHAT,
        DM,
        BOTH,
        DISABLED
    }
}