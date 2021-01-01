using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Confirmation;
using Interactivity.Selection;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Discord.Memory;

namespace Kaguya.Discord.Commands.Games
{
    [Module(CommandModule.Games)]
    [Group("crossgamble")]
    [Alias("cg")]
    [RequireUserPermission(ChannelPermission.AddReactions)]
    [RequireUserPermission(GuildPermission.AddReactions)]
    public class CrossGambling : KaguyaBase<CrossGambling>
    {
        private readonly ILogger<CrossGambling> _logger;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly DiscordShardedClient _client;

        public CrossGambling(ILogger<CrossGambling> logger, KaguyaUserRepository kaguyaUserRepository, InteractivityService interactivityService,
            DiscordShardedClient client) : base(logger)
        {
            _logger = logger;
            _kaguyaUserRepository = kaguyaUserRepository;
            _client = client;
        }

        [Command(RunMode = RunMode.Async)]
        [Summary("A multiplayer gambling game, inspired by World of Warcraft gold gambling.\n\n" +
                 "- Minimum 2 players.\n" +
                 "- Everyone who plays must have at least `<max amount>` points to play.\n" +
                 "- The game host starts a session by executing the command with a `<max amount>` points value. " +
                 "Other players can join the game through clicking on the reaction attached to the message.\n" +
                 "- All users who join the game will be randomly assigned a number between 1 and the `<max amount>`.\n\n" +
                 "The person with the **highest roll wins the difference between their roll and the lowest roll** in the game.\n" +
                 "For example: If the `<max amount>` is 10000, the highest roll is 7500, and the lowest roll is 1000, whoever " +
                 "rolled 7500 wins 6500 of the loser's points.")]
        [Remarks("<max amount>")]
        [Example("30000")]
        [Example("1000000")]
        public async Task CrossGamblingCommand(int maxAmount)
        {
            const int DELAY_SECONDS = 30;
            var hostUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);

            if (hostUser.Points < maxAmount)
            {
                await SendBasicErrorEmbedAsync($"You cannot start a game for this many points. You only have `{hostUser.Points:N0}` points.");

                return;
            }

            if (ActiveMultiplayerSessions.IsActive(Context.Channel.Id, MultiplayerGameType.CrossGambling))
            {
                await SendBasicErrorEmbedAsync("This channel already has an active multiplayer game session!");

                return;
            }

            var gambleSession = new CrossGamblingSession(Context.Channel.Id, MultiplayerGameType.CrossGambling);
            ActiveMultiplayerSessions.AddSession(gambleSession);
            
            IEmote[] reactions =
            {
                new Emoji("💰"),
                new Emoji("🙅"),
                new Emoji("⌛"),
                new Emoji("⛔")
            };
            
            var embed = new KaguyaEmbedBuilder(Color.Gold)
            {
                Title = "Cross-Gambling",
                Description = $"A cross-gambling game has just begun! (2-25 players)\n" +
                              $"{reactions[0]} - Click to join! " + $"Requires {maxAmount:N0} points".AsBold() + "\n" +
                              $"{reactions[1]} - Click to leave the game.\n" +
                              $"{reactions[2]} - Starts game immediately (only usable by {Context.User.Mention}).\n" +
                              $"{reactions[3]} - Cancels the game session (only usable by {Context.User.Mention}).",
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Started by: {Context.User} | Click the reaction to join!\n" +
                           $"Automatically starts in {DELAY_SECONDS} seconds."
                },
                Timestamp = DateTimeOffset.Now.AddSeconds(DELAY_SECONDS)
            };
            
            RestUserMessage curMsg = await SendEmbedAsync(embed);
            await curMsg.AddReactionsAsync(reactions);
            
            // Move to another thread.
            List<KaguyaUser> gamblerUserAccs = new List<KaguyaUser>();
            await Task.Run(async () =>
            {
                bool jump = false;
                bool abort = false;
                
                _client.ReactionAdded += OnClientReactionAdded;
                
                Task OnClientReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
                {
                    bool validSysReaction = cacheable.HasValue &&
                                            channel.Id == Context.Channel.Id &&
                                            reaction.UserId == Context.User.Id &&
                                            cacheable.Value.Id == curMsg.Id;
                    if (validSysReaction && reaction.Emote.Name.Equals(reactions[2].Name))
                    {
                        jump = true;
                    }
                    else if (validSysReaction && reaction.Emote.Name.Equals(reactions[3].Name))
                    {
                        abort = true;
                    }

                    return Task.CompletedTask;
                }

            #region Delay Loop
                int secondsCount = 0;
                while (true)
                {
                    if (abort || jump || secondsCount >= DELAY_SECONDS)
                    {
                        break;
                    }
                    
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    secondsCount++;
                }
            #endregion
                
                ActiveMultiplayerSessions.RemoveSession(gambleSession);

                if (abort)
                {
                    await AbortAsync(curMsg, "The session has been aborted.", OnClientReactionAdded);
                }
                
                List<IUser> gamblerUsers = (await curMsg.GetReactionUsersAsync(reactions[0], 25).FlattenAsync()).ToList();
                IEnumerable<IUser> reversedUsers = await curMsg.GetReactionUsersAsync(reactions[1], 25).FlattenAsync();
                gamblerUsers.RemoveAll(x => reversedUsers.Any(y => x.Id == y.Id));

                foreach (IUser newUser in gamblerUsers)
                {
                    var kaguyaUser = await _kaguyaUserRepository.GetOrCreateAsync(newUser.Id);

                    if (kaguyaUser.Points > maxAmount)
                    {
                        gamblerUserAccs.Add(kaguyaUser);
                    }
                }

                gamblerUsers.RemoveAll(x => !gamblerUserAccs.Any(y => x.Id == y.UserId));
                
                if (gamblerUsers.Count < 2)
                {
                    await AbortAsync(curMsg, "There were not enough players to start this game. Aborting!", OnClientReactionAdded);

                    return;
                }
                
                await ProcessUsers(gamblerUsers, curMsg, maxAmount);
            });
        }

        private async Task ProcessUsers(IEnumerable<IUser> users, RestUserMessage toModify, int maxAmount)
        {
            Random r = new Random();
            Dictionary<IUser, int> userRolls = GetUserRolls(users, maxAmount, r);

            var ordered = userRolls.OrderByDescending(x => x.Value).ToList();
            var highRoll = ordered.FirstOrDefault();
            var lowRoll = ordered.LastOrDefault();

            if (highRoll.Key.Equals(lowRoll.Key))
            {
                await SafeSetInactiveEmbedAsync(toModify);
                await SendBasicErrorEmbedAsync($"{highRoll.Key.Mention} is the only user in the game! " +
                                               $"Cancelling...");

                return;
            }
            
            int difference = highRoll.Value - lowRoll.Value;

            if (difference == 0)
            {
                await SafeSetInactiveEmbedAsync(toModify);
                await SendBasicEmbedAsync("Wow! There was a tie, what an anomaly!", Color.Magenta, false);
                
                return;
            }

            if (!await SafeSetInactiveEmbedAsync(toModify))
            {
                await SendBasicErrorEmbedAsync($"Uh oh, it looks like the original message was deleted or corrupted. " +
                                               $"This game has been aborted!");

                return;
            }
            
            KaguyaUser winnerUser = await _kaguyaUserRepository.GetOrCreateAsync(highRoll.Key.Id);
            KaguyaUser loserUser = await _kaguyaUserRepository.GetOrCreateAsync(lowRoll.Key.Id);
            
            await UpdateWinnerLoserPointsAsync(winnerUser, difference, loserUser);

            var finalEmbed = new KaguyaEmbedBuilder(Color.Green)
                             .WithTitle("Cross Gambling: Result")
                             .WithDescription($"Maximum roll: {maxAmount.ToString("N0").AsBold()}\n\n" +
                                              $"{highRoll.Key.Mention} rolled {highRoll.Value.ToString("N0").AsBold()}.\n" +
                                              $"{lowRoll.Key.Mention} rolled {lowRoll.Value.ToString("N0").AsBold()}.\n\n" +
                                              $"{highRoll.Key.Mention} won {difference.ToString("N0").AsBold()} of {lowRoll.Key.Mention}'s points!")
                             .WithFooter($"{highRoll.Key.Username} now has {winnerUser.Points:N0} points! (+{difference:N0})\n" +
                                         $"{lowRoll.Key.Username} now has {loserUser.Points:N0} points. (-{difference:N0})");

            await SendEmbedAsync(finalEmbed);
        }

        private static Dictionary<IUser, int> GetUserRolls(IEnumerable<IUser> users, int maxAmount, Random r)
        {
            var userRolls = new Dictionary<IUser, int>();

            foreach (IUser user in users)
            {
                if (user.IsBot)
                {
                    continue;
                }

                int roll = r.Next(maxAmount - 1) + 1;
                userRolls.Add(user, roll);
            }

            return userRolls;
        }

        private async Task UpdateWinnerLoserPointsAsync(KaguyaUser winnerUser, int difference, KaguyaUser loserUser)
        {
            winnerUser.AdjustPoints(difference);
            loserUser.AdjustPoints(-difference);

            await _kaguyaUserRepository.UpdateRange(new[]
            {
                winnerUser,
                loserUser
            });
        }

        private async Task AbortAsync(RestUserMessage curMsg, string abortReason, 
            Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> reactionTask)
        {
            _client.ReactionAdded -= reactionTask;

            await SetToInactiveEmbedAsync(curMsg);
            await SendBasicErrorEmbedAsync(abortReason);
        }

        private async Task<bool> SafeSetInactiveEmbedAsync(RestUserMessage curMsg)
        {
            try
            {
                await SetToInactiveEmbedAsync(curMsg);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}