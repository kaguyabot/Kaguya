using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Threading.Tasks;
using LinqToDB.Mapping;

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
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);

            if (type == null && channel == null)
            {
                var curChatType = user.ExpChatNotificationType;
                var curDmType = user.ExpDmNotificationType;

                var userPref = $"{Context.User.Mention}, here are your current EXP level-up notification preferences:\n\n" +
                               $"Chat reply: `{curChatType.Humanize(LetterCasing.Title)}`\n" +
                               $"DM: `{curDmType.Humanize(LetterCasing.Title)}`";

                await SendBasicSuccessEmbedAsync(userPref);
                return;
            }

            var expType = GetExpType(type);
            var expChannel = GetChannelPref(channel);

            switch (expChannel)
            {
                case ExpChannel.Chat:
                    user.ExpChatNotificationTypeNum = (int)expType;
                    break;
                case ExpChannel.DM:
                    user.ExpDmNotificationTypeNum = (int)expType;
                    break;
                case ExpChannel.Both:
                    user.ExpChatNotificationTypeNum = (int)expType;
                    user.ExpDmNotificationTypeNum = (int)expType;
                    break;
                case ExpChannel.Disabled:
                    user.ExpChatNotificationTypeNum = (int)ExpType.None;
                    user.ExpDmNotificationTypeNum = (int)ExpType.None;
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

        private ExpChannel GetChannelPref(string channel)
        {
            return channel.ToLower() switch
            {
                "chat" => ExpChannel.Chat,
                "dm" => ExpChannel.DM,
                "both" => ExpChannel.Both,
                "disabled" => ExpChannel.Disabled,
                _ => throw new ArgumentOutOfRangeException("The route parameter must be set " +
                                                           "to either `chat`, `dm`, `both`, or `disabled`.")
            };
        }

        private ExpType GetExpType(string type)
        {
            return type.ToLower() switch
            {
                "global" => ExpType.Global,
                "server" => ExpType.Server,
                "both" => ExpType.Both,
                "none" => ExpType.None,
                _ => throw new ArgumentOutOfRangeException("The level-up type parameter must be either " +
                                                           "`server`, `global`, `both`, or `none`.")
            };
        }
    }

    public enum ExpType
    {
        Global,
        Server,
        Both,
        None
    }

    public enum ExpChannel
    {
        Chat,
        DM,
        Both,
        Disabled
    }
}