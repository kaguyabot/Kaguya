using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Helpers;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using MoreLinq;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class CreateReactionRole : KaguyaBase
    {
        [UtilityCommand]
        [Command("CreateReactionRole")]
        [Alias("crr")]
        [Summary("Allows a user to add a emote-role pair to a message in the form of a reaction. Users who click on " +
                 "the reaction will then be given the role paired to the emote. When a user removes " +
                 "their reaction, the role will be removed from them.\n\n" +
                 "Multiple reaction roles can be created at once by placing a new `emote` and `role` " +
                 "together on a new line.\n\n" +
                 "Note: The emote assigned must either be a Discord emote uploaded to this server. " +
                 "Emojis are not yet supported.")]
        [Remarks("<message ID> <emoji> <role> {[emoji] [role] ...}")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        public async Task Command(ulong messageId, Emote emote, IRole role, [Remainder] string args = null)
            => await Command(messageId, (ITextChannel)Context.Channel, emote, role, args);
        
        // Main command logic
        [UtilityCommand]
        [Command("CreateReactionRole")]
        [Alias("crr")]
        [Summary("Allows a user to add a emote-role pair to a message in the form of a reaction. Users who click on " +
                 "the reaction will then be given the role paired to the emote. When a user removes " +
                 "their reaction, the role will be removed from them.\n\n" +
                 "Multiple reaction roles can be created at once by placing a new `emote` and `role` " +
                 "together on a new line.\n\n" +
                 "Note: The emote assigned must either be a Discord emote uploaded to this server. " +
                 "Emojis are not yet supported.")]
        [Remarks("<message ID> <emoji> <role> {[emoji] [role] ...}")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        public async Task Command(ulong messageId, ITextChannel channel, Emote emote, IRole role, [Remainder]string args = null)
        {
            IMessage message = await channel.GetMessageAsync(messageId);
            var emoteRolePair = new Dictionary<Emote, IRole>() { {emote, role} };
            var argLines = args?.Split("\n");
            
            if (!string.IsNullOrWhiteSpace(args))
            {
                // If there aren't an even amount of args, we know the user messed up.
                if (args.Split(' ').Length % 2 != 0)
                {
                    throw new KaguyaSupportException("There were an invalid amount of arguments provided. " +
                                                     "Note that for each additional entry, there must be an " +
                                                     "emote followed by a role.");
                }

                foreach (var line in argLines)
                {
                    bool validEmote = false;
                    bool validRole = false;
                    var lineSplits = line.Split(" ");

                    if (Emote.TryParse(lineSplits[0], out Emote emoteResult))
                        validEmote = true;
                    
                    if (MentionUtils.TryParseRole(lineSplits[1], out ulong roleId))
                    {
                        if (Context.Guild.GetRole(roleId) != null)
                        {
                            validRole = true;
                        }
                    }

                    if (validEmote == false || validRole == false)
                    {
                        throw new KaguyaSupportException("Failed to parse a valid emote and role from provided " +
                                                         $"input: '{line}'\n\n" +
                                                         $"Note that the emote must be from this server only and " +
                                                         $"cannot be a standard emoji.");
                    }
                    
                    emoteRolePair.Add(emoteResult, Context.Guild.GetRole(roleId));
                }
            }

            var respSb = new StringBuilder();
            foreach (var pair in emoteRolePair)
            {
                var pEmote = pair.Key;
                var pRole = pair.Value;
                
                var rr = new ReactionRole(pEmote.Id, pRole.Id, message.Id, Context.Guild.Id);
                await message.AddReactionAsync(pEmote);

                var possibleMatch = await DatabaseQueries.GetFirstMatchAsync<ReactionRole>(x =>
                    x.EmoteId == pEmote.Id && x.RoleId == pRole.Id && x.MessageId == rr.MessageId);

                if (possibleMatch != null)
                {
                    throw new KaguyaSupportException($"The emote '{emote}' has already been assigned to role {role} " +
                                                     "as a reaction role.");
                }

                try
                {
                    await DatabaseQueries.InsertAsync(rr);
                    respSb.AppendLine($"Successfully linked {pEmote} to {pRole.Mention}");
                }
                catch (Exception e)
                {
                    throw new KaguyaSupportException("An error occurred when inserting the reaction role " +
                                                     "into the database.\n\n" +
                                                     $"Emote: {pEmote}\n" +
                                                     $"Role: {pRole}");
                }
            }

            var embed = new KaguyaEmbedBuilder(EmbedColor.YELLOW)
            {
                Title = "Kaguya Reaction Roles",
                Description = respSb.ToString(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = "This message will self destruct in 30 seconds..."
                }
            };

            //todo: Come back and continue to fix.
            await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(30));
        }
    }
}