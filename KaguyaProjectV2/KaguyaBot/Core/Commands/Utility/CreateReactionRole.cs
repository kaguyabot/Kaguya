﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

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
                 "Note: The emote assigned must be a Discord emote uploaded to this server. " +
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
                 "Note: The emote assigned must be a Discord emote uploaded to this server. " +
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

                var rr = new ReactionRole
                {
                    EmoteId = emote.Id,
                    MessageId = message.Id,
                    RoleId = role.Id,
                    ServerId = Context.Guild.Id
                };

                if (pRole.IsManaged)
                {
                    throw new KaguyaSupportException($"Role '{pRole.Name}' is managed by an integration or a bot. It may not be " +
                                                     "assigned to users. Therefore, they may not be assigned to " +
                                                     "reaction roles either.");
                }
                
                try
                {
                    await message.AddReactionAsync(pEmote);
                }
                catch (Discord.Net.HttpException e)
                {
                    if (e.HttpCode == HttpStatusCode.BadRequest)
                    {
                        await SendBasicErrorEmbedAsync($"An error occurred when attempting to make the reaction role " +
                                                       $"for the '{pEmote.Name}' emote. This error occurs when Discord " +
                                                       $"doesn't know how to process an emote. This can happen if you " +
                                                       $"copy/paste the emote into the Discord text box instead of " +
                                                       $"manually typing out the emote yourself. Discord is really " +
                                                       $"finnicky when it comes to emotes.");
                        return;
                    }

                    await SendBasicErrorEmbedAsync($"An unknown error occurred.\n\nDetails: {e}");
                }

                var possibleMatch = await DatabaseQueries.GetFirstMatchAsync<ReactionRole>(x =>
                    x.EmoteId == pEmote.Id && x.RoleId == pRole.Id && x.MessageId == rr.MessageId);
                var messageReactions = message.Reactions;
                
                // If the reaction is in the database, and Kaguya has a emote-role pair for this emote, throw an error.
                if (possibleMatch != null && messageReactions.Keys.Contains(pEmote) && 
                    messageReactions.GetValueOrDefault(pEmote).IsMe)
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

            await ReplyAndDeleteAsync(null, false, embed.Build(), TimeSpan.FromSeconds(30));
        }
    }
}