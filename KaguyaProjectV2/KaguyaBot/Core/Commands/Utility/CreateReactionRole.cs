using System;
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
        public static event Action<IEnumerable<ReactionRole>> UpdatedCache; 
        
        [UtilityCommand]
        [Command("CreateReactionRole")]
        [Alias("crr")]
        [Summary("Allows a user to add a emote-role pair to a message in the form of a reaction. Users who click on " +
                 "the reaction will then be given the role paired to the emote. When a user removes " +
                 "their reaction, the role will be removed from them.\n\n" +
                 "Multiple reaction roles can be created at once by placing a new `emote` and `role` " +
                 "together on a new line.\n\n" +
                 "Note: The emote assigned must be a Discord emote uploaded to this server. " +
                 "Standard Emojis are not supported.\n\n" +
                 "If your role has spaces, don't forget to wrap it in double quotes like so: `\"My Role\"`\n\n" +
                 "Subsequent emote-role pairs can be added seamlessly.")]
        [Remarks("<message ID> [channel] <emote> <role> {emote role} {...}\n" +
                 "588369719132684296 :Banger: OG\n" +
                 "588369719132684296 :PepeJam: \"DJ Master\" :PepeLaugh: Comedian\n" +
                 "685369725551652584 #role-channel :color-blue: \"Aqua Blue\" :color-red: Crimson")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        public async Task Command(ulong messageId, Emote emote, IRole role, params string[] args)
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
                 "Standard Emojis are not supported.\n\n" +
                 "If your role has spaces, don't forget to wrap it in double quotes like so: `\"My Role\"`\n\n" +
                 "Subsequent emote-role pairs can be added seamlessly.")]
        [Remarks("<message ID> [channel] <emote> <role> {emote role} {...}\n" +
                 "588369719132684296 :Banger: OG\n" +
                 "588369719132684296 :PepeJam: \"DJ Master\" :PepeLaugh: Comedian\n" +
                 "685369725551652584 #role-channel :color-blue: \"Aqua Blue\" :color-red: Crimson")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.AddReactions)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.AddReactions)]
        public async Task Command(ulong messageId, ITextChannel channel, Emote emote, IRole role, params string[] args)
        {
            IMessage message = await channel.GetMessageAsync(messageId);
            var emoteRolePair = new Dictionary<Emote, IRole> { {emote, role} };
            
            if (args.Any())
            {
                // If there aren't an even amount of args, we know the user messed up.
                if (args.Length % 2 != 0)
                {
                    throw new KaguyaSupportException("There were an invalid amount of additional arguments provided. " +
                                                     "Note that for each additional entry, there must be an " +
                                                     "emote followed by a role.");
                }

                string[][] emoteRolePairs = new string[args.Length / 2][];

                int count = 0;
                for (int i = 0; i < args.Length; i += 2)
                {
                    var rolePair = new string[2];
                    rolePair[0] = args[i];
                    rolePair[1] = args[i + 1];

                    emoteRolePairs[count] = rolePair;
                    count++;
                }
                
                foreach (var pair in emoteRolePairs)
                {
                    string emoteText = pair[0];
                    string roleText = pair[1];
                    
                    bool validEmote = false;
                    bool validRole = false;

                    if (Emote.TryParse(emoteText, out Emote emoteResult))
                        validEmote = true;

                    IRole roleResult = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleText.ToLower());
                    if (roleResult != null)
                    {
                        validRole = true;
                    }
                    
                    if (validEmote == false)
                    {
                        throw new KaguyaSupportException("Failed to parse a valid emote from the provided " +
                                                         $"input: Emote: '{emoteText}'\n\n" +
                                                         $"Note that the emote must be from this server only and " +
                                                         $"cannot be a standard emoji.");
                    }

                    if (validRole == false)
                    {
                        throw new KaguyaSupportException("Failed to parse a valid role from the provided " +
                                                         $"input: Role: '{roleText}'");
                    }
                    
                    emoteRolePair.Add(emoteResult, Context.Guild.GetRole(roleResult.Id));
                }
            }

            if (message == null)
            {
                throw new KaguyaSupportException("The message with this ID could not be found in the specified channel. " +
                                                 "You must specify the 'channel' argument for this command if you are " +
                                                 "executing the command from another channel. \n\n" +
                                                 $"Example: '{{prefix}}crr {messageId} {{#some-channel}} ...'");
            }

            var cacheToSend = new List<ReactionRole>(emoteRolePair.Count);

            int cacheListIndex = 0;
            var respSb = new StringBuilder();
            foreach (var pair in emoteRolePair)
            {
                var pEmote = pair.Key;
                var pRole = pair.Value;

                var rr = new ReactionRole
                {
                    EmoteId = pEmote.Id,
                    MessageId = message.Id,
                    RoleId = pRole.Id,
                    ServerId = Context.Guild.Id
                };

                if (pRole.IsManaged)
                {
                    throw new KaguyaSupportException($"Role '{pRole.Name}' is managed by an integration or a bot. It may not be " +
                                                     "assigned to users. Therefore, they may not be assigned to " +
                                                     "reaction roles either.");
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
                    await message.AddReactionAsync(pEmote);
                    await DatabaseQueries.InsertAsync(rr);
                    
                    cacheToSend.Insert(cacheListIndex, rr);
                    respSb.AppendLine($"Successfully linked {pEmote} to {pRole.Mention}");
                }
                catch (Discord.Net.HttpException e)
                {
                    if (e.HttpCode == HttpStatusCode.BadRequest)
                    {
                        throw new KaguyaSupportException($"An error occurred when attempting to make the reaction role " +
                                                         $"for the '{pEmote.Name}' emote. This error occurs when Discord " +
                                                         $"doesn't know how to process an emote. This can happen if you " +
                                                         $"copy/paste the emote into the Discord text box instead of " +
                                                         $"manually typing out the emote yourself. Discord is really " +
                                                         $"finnicky when it comes to emotes.");
                    }

                    throw new KaguyaSupportException($"An unknown error occurred.\n\n" +
                                                     $"Exception Message: {e.Message}\nInner Exception: {e.InnerException}");
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
            };

            UpdatedCache?.Invoke(cacheToSend);
            await SendEmbedAsync(embed);
        }
    }
}