using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Application;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency.Poker
{
    public class PokerGame : KaguyaBase
    {
        private const int SUIT_LEN = 13;
        private const int TIMEOUT = 60;
        private const EmbedColor POKER_COLOR = EmbedColor.MAGENTA;
        
        [CurrencyCommand]
        [Command("Poker", RunMode = RunMode.Async)]
        [Alias("pk")]
        [Summary("Starts a game of Texas Hold'em Poker between you and me, the dealer!\n\n" +
                 "__**How to Play:**__\n" +
                 "- In order to play, you must bet at least 500 points, up to 50,000 points. The dealer will also match " +
                 "your bet, so if you win, you will win `2.15x` whatever you put into the pot.\n" +
                 "- Each player (you and the dealer) will be dealt two cards. You will not know what the dealer's hand is.\n" +
                 "- After the hands are dealt, there will be a `flop` of 3 random cards. At this point, you can either " +
                 "`fold`, `raise`, `call`, or `check` if you were raised. " +
                 "Be careful - if the dealer thinks they have a good hand, they will raise you! " +
                 "If you fail to `call` the dealer's raise, you forfeit your winnings and automatically fold.\n" +
                 "- After you have made a decision, the next round of betting will begin with a new card displayed: " +
                 "the `turn card`. The same options will be displayed again.\n" +
                 "- Finally, the `river card` will be displayed. This is the last round of betting.\n\n")]
        [Remarks("<points>")]
        public async Task Command(int points)
        {
            User user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            GuildEmote cardBackEmote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 761212621676085278);

            if (MemoryCache.ActivePokerSessions.Contains(Context.User.Id))
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} You are already in a poker game. " +
                                               $"Finish the first one to play again!");
                return;
            }
            
            #region Input Serialization {...}
            if (cardBackEmote == null)
            {
                throw new KaguyaSupportException("The Kaguya Cardback emote could not be found. Please report this " +
                                                 "issue in our support Discord.");
            }
            
            if (user.Points < points)
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} you do not have enough points.\n\n" +
                                               $"You have {user.Points:N0} points.");
                return;
            }

            if (points < 500)
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} you must bet at least `500` points.");
                return;
            }

            if (!user.IsPremium && points > 50000)
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} you may only bet up to `50,000` points.");
                return;
            }

            if (user.IsPremium && points > 500000)
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} you may only bet up to `500,000` points.");
                return;
            }
            #endregion

            PokerData pokerData = new PokerData(points);
            // We must only add to the cache after the intial serialization has occurred.
            // Otherwise, the user will be unable to start a new Poker game.
            MemoryCache.ActivePokerSessions.Add(user.UserId);

            // We don't set PokerData.dealerPointsBet yet because the pot already multiplies the user's bet 
            // by the multiplier. That value should only be set when the dealer AI makes a decision to bet/raise.
            pokerData.userPointsBet += points;
            pokerData.UpdatePot();

            Hand playerHand = GeneratePlayerHand();
            Hand dealerHand = GeneratePlayerHand();
            
            List<Card> communityCards = GenerateFlop().ToList(); // We start with a flop, then add onto this later.
            Hand communityHand = new Hand(communityCards);

            // Turn 1 - display basic information to user (their hand + the flop)
            var embed = OverviewEmbed(pokerData, playerHand, dealerHand, communityHand, true, false, true, true, false);
            var data = EmbedReactionData(embed, pokerData, TIMEOUT, true, false, true, true, false);
            await InlineReactionReplyAsync(data);

            // Subsequent turns
            Func<PokerGameEventArgs, Task> pokerEventHandler = PokerTurnHandler(pokerData, playerHand, dealerHand, communityHand, user);
            PokerEvent.OnTurnEnd += pokerEventHandler;
            PokerEvent.OnGameFinished += () => Task.Run(() => PokerEvent.OnTurnEnd -= pokerEventHandler);
        }

        private Func<PokerGameEventArgs, Task> PokerTurnHandler(PokerData pokerData, Hand playerHand, 
            Hand dealerHand, Hand communityHand, User user)
        {
            Func<PokerGameEventArgs, Task> pokerEventHandler = async e =>
            {
                bool lastTurn = communityHand.Length == 5;

                if (!lastTurn && e.Action == PokerGameAction.FOLD)
                    lastTurn = true;

                if (!lastTurn)
                    communityHand.AddCard(GenerateRandomCard());

                pokerData.UpdatePot();

                switch (e.Action)
                {
                    case PokerGameAction.CHECK:
                        if (lastTurn)
                            break;

                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} You have decided to check.");

                        var checkEmbed = CheckEmbed(pokerData, playerHand, dealerHand, communityHand, false, Context);
                        var callData = EmbedReactionData(checkEmbed, pokerData, TIMEOUT, true, false, true, true, false);
                        await InlineReactionReplyAsync(callData);
                        break;
                    case PokerGameAction.RAISE:
                        int raisePointsDelta = user.Points - pokerData.userPointsBet;
                        // todo: Make pretty
                        await ReplyAsync($"{Context.User.Mention} how many additional points do you want to raise?\n" +
                                         $"You may bet a maximum of `{raisePointsDelta:N0}` points.\n\n" +
                                         "*You have 60 seconds to reply.*\n" +
                                         "*Enter an exact integer.*");
                        int raisePoints;
                        while (true)
                        {
                            SocketMessage response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                            bool validResponse = int.TryParse(response.Content, out int res);
                            if(!validResponse)
                            {
                                await ReplyAsync($"{Context.User.Mention} you must enter an exact integer! You have 60 seconds. " +
                                                 $"Please try again.");
                                continue;
                            }

                            if(res > raisePointsDelta)
                            {
                                await ReplyAsync($"{Context.User.Mention} this number of points is greater than your " +
                                                 $"total points balance! Please enter a smaller number! " +
                                                 $"(Minimum: `500`, Maximum: `{raisePointsDelta:N0}`)");
                                continue;
                            }

                            if (res < 500)
                            {
                                await ReplyAsync($"{Context.User.Mention} you must bet at least 500 points!");
                                continue;
                            }

                            raisePoints = res;
                            break;
                        }

                        pokerData.userPointsBet += raisePoints;
                        pokerData.UpdatePot();

                        if (lastTurn)
                        {
                            await ReplyAsync($"{Context.User.Mention} you have raised the pot by " +
                                             $"{raisePoints:N0} points! This is the last turn, good luck...!");
                            break;
                        }
                        // Re-raise / call AI could go here.
                        var raiseEmbed = RaiseEmbed(pokerData, playerHand, dealerHand, communityHand, raisePoints, Context);
                        var raiseData = EmbedReactionData(raiseEmbed, pokerData, TIMEOUT, true, false, true, true, false, user,
                            raisePoints);
                        await InlineReactionReplyAsync(raiseData);
                        break;
                    case PokerGameAction.FOLD:
                        await InsertGambleHistory(user, false, pokerData);
                        await SetUserPoints(user, false, pokerData);
                        await SendEmbedAsync(FoldEmbed(pokerData, playerHand, dealerHand, communityHand, user, Context));
                        RemoveGameFromCache(user.UserId);
                        PokerEvent.GameFinishedTrigger();
                        return;
                }

                if (lastTurn)
                {
                    RemoveGameFromCache(Context.User.Id);

                    HandRanking playerRanking = PokerData.IdentifyRanking(playerHand, communityHand);
                    HandRanking dealerRanking = PokerData.IdentifyRanking(dealerHand, communityHand);

                    // Here, '<' is used because the stronger hands are of a lower integer value.
                    bool playerWinner = playerRanking < dealerRanking;
                    bool rankingTie = playerRanking == dealerRanking;
                    bool push = false;
                    
                    if (rankingTie)
                    {
                        playerWinner = PokerData.PlayerHasHighestCard(playerHand, dealerHand);

                        if (!playerWinner)
                        {
                            // This basically says: "The player and the dealer
                            // have the exact same hand with different suits".
                            if (playerHand.Cards.Sum(x => x.NumericValue) == dealerHand.Cards.Sum(x => x.NumericValue))
                                push = true;
                        }
                    }

                    await InsertGambleHistory(user, playerWinner, pokerData);
                    await SetUserPoints(user, playerWinner, pokerData);
                    
                    if (playerWinner)
                    {
                        await SendEmbedAsync(PlayerWinnerEmbed(pokerData, playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking, user, Context));
                    }
                    else if (push)
                    {
                        await SendEmbedAsync(TieEmbed(pokerData, playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking, user, Context));
                    }
                    else
                    {
                        await SendEmbedAsync(LoseEmbed(pokerData, playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking, user, Context));
                    }

                    PokerEvent.GameFinishedTrigger();
                }
            };
            return pokerEventHandler;
        }

        private static async Task SetUserPoints(User user, bool playerWinner, PokerData pokerData)
        {
            user.Points -= pokerData.userPointsBet;

            if (!playerWinner)
            {
                await DatabaseQueries.UpdateAsync(user);
                return;
            }
            
            user.Points += (int) pokerData.pot;
            await DatabaseQueries.UpdateAsync(user);
        }

        private static async Task InsertGambleHistory(User user, bool playerWinner, PokerData pokerData)
        {
            var gh = new GambleHistory
            {
                UserId = user.UserId,
                Action = GambleAction.POKER,
                Bet = pokerData.userPointsBet,
                Payout = playerWinner ? (int)pokerData.pot : -pokerData.userPointsBet,
                Roll = -1,
                Time = DateTime.Now.ToOADate(),
                Winner = playerWinner
            };
            await DatabaseQueries.InsertAsync(gh);
        }

        #region Embed helper methods {...}
        private static KaguyaEmbedBuilder OverviewEmbed(PokerData pokerData, Hand playerHand, Hand dealerHand, Hand communityHand,
            bool canCheck, bool canCall, bool canRaise, bool canFold, bool lastTurn)
        {
            var embed = new KaguyaEmbedBuilder(POKER_COLOR)
            {
                Title = "Kaguya Poker",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, lastTurn),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(pokerData, canCheck, canCall, canRaise, canFold, lastTurn, null)
                }
            };
            return embed;
        }

        private static KaguyaEmbedBuilder PlayerWinnerEmbed(PokerData pokerData, Hand playerHand, Hand dealerHand, Hand communityHand, 
            HandRanking playerHandRanking, HandRanking dealerHandRanking, User user, ICommandContext context)
        {
            return new KaguyaEmbedBuilder(EmbedColor.GREEN)
            {
                Title = "Kaguya Poker: Winner!",
                Description = $"{context.User.Mention} You won!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(pokerData, false, false, false, false, true, true, true, true, user) + "\n" +
                           $"Your hand: {playerHandRanking.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHandRanking.Humanize(LetterCasing.Title)}"
                }
            };
        }
        
        private static KaguyaEmbedBuilder TieEmbed(PokerData pokerData, Hand playerHand, Hand dealerHand, Hand communityHand,
            HandRanking playerHr, HandRanking dealerHr, User user, ICommandContext context)
        {
            return new KaguyaEmbedBuilder(EmbedColor.GOLD)
            {
                Title = "Kaguya Poker: Push!",
                Description = $"{context.User.Mention} There was a tie! How extraordinary!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(pokerData, false, false, false, false, true, null, true, true, user) + "\n" +
                           $"Your hand: {playerHr.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHr.Humanize(LetterCasing.Title)}"
                }
            };
        }
        
        private static KaguyaEmbedBuilder LoseEmbed(PokerData pokerData, Hand playerHand, Hand dealerHand, Hand communityHand,
            HandRanking playerHr, HandRanking dealerHr, User user, ICommandContext context)
        {
            return new KaguyaEmbedBuilder(EmbedColor.LIGHT_PURPLE)
            {
                Title = "Kaguya Poker: Loser!",
                Description = $"{context.User.Mention} You lost! Better luck next time...",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(pokerData, false, false, false, false, true, null, true, true, user) + "\n" +
                           $"Your hand: {playerHr.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHr.Humanize(LetterCasing.Title)}"
                }
            };
        }

        private static KaguyaEmbedBuilder RaiseEmbed(PokerData pokerData, Hand pHand, Hand dHand, Hand cHand, int raisePoints,
            ICommandContext context)
        {
            return new KaguyaEmbedBuilder(EmbedColor.LIGHT_BLUE)
            {
                Title = "Kaguya Poker: Raise!",
                Description = $"{context.User.Mention} You have raised the pot by {raisePoints:N0} points!",
                Fields = PokerTableEmbedFields(pHand, dHand, cHand, false),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(pokerData, true, false, true, true, false, null)
                }
            };
        }
        
        private static KaguyaEmbedBuilder FoldEmbed(PokerData pokerData, Hand pHand, Hand dHand, Hand cHand, User user, ICommandContext context)
        {
            return new KaguyaEmbedBuilder(EmbedColor.GRAY)
            {
                //todo: Better phrasing? lol
                Title = "Kaguya Poker: Fold!",
                Description = $"{context.User.Mention} You have folded your hand!",
                Fields = PokerTableEmbedFields(pHand, dHand, cHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(pokerData, false, false, false, false, true, false, true, true, user)
                }
            };
        }

        /// <summary>
        /// The embed sent after each response from the user (check, fold, etc.)
        /// </summary>
        /// <param name="playerHand"></param>
        /// <param name="dealerHand"></param>
        /// <param name="communityHand"></param>
        /// <param name="lastTurn"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private static KaguyaEmbedBuilder CheckEmbed(PokerData pokerData, Hand playerHand, Hand dealerHand, 
            Hand communityHand, bool lastTurn, ICommandContext context)
        {
            var embed = new KaguyaEmbedBuilder(POKER_COLOR)
            {
                Title = $"Kaguya Poker: Check!",
                Description = $"{context.User.Mention} You have decided to check. Let's keep going!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, lastTurn),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(pokerData, true, false, true, true, lastTurn, null)
                }
            };

            return embed;
        }
        
        private static List<EmbedFieldBuilder> PokerTableEmbedFields(Hand playerHand, Hand dealerHand, 
            Hand communityHand, bool showDealerHand)
        {
            var dealerField = new EmbedFieldBuilder
            {
                Name = "Dealer Hand",
                IsInline = true
            };

            dealerField.Value = showDealerHand
                ? dealerHand.ToString()
                : $"{PokerData.cardbackEmote}? {PokerData.cardbackEmote}?";
            
            return new List<EmbedFieldBuilder>
            {
                // We hide the dealer's hand from view.
                dealerField,
                new EmbedFieldBuilder
                {
                    Name = "Player Hand",
                    Value = playerHand.ToString(),
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Table Cards",
                    Value = communityHand.Cards.ToReadable(),
                    IsInline = false
                }
            };
        }

        private static string GetOptionsFooter(PokerData pokerData, bool canCheck, bool canCall, bool canRaise, 
            bool canFold, bool lastTurn, bool? winner, bool showPot = true, bool showUserPoints = false, User user = null)
        {
            string potStr = $"Current Pot: {(int) pokerData.pot:N0}\n";
            var sb = new StringBuilder();
            if (canCheck)
                sb.Append(PokerData.rcCheck + "Check,");
            if (canCall)
                sb.Append(PokerData.rcCall + "Call,");
            if (canRaise)
                sb.Append(PokerData.rcRaise + "Raise,");
            if (canFold)
                sb.Append(PokerData.rcFold + "Fold,");

            var fSb = new StringBuilder(potStr);

            if (showUserPoints)
            {
                if (user == null)
                {
                    throw new NullReferenceException("There must be a user passed into this method " +
                                                     "if showUserPoints is equal to true.");
                }

                char? modifier = null;
                if (winner.HasValue)
                {
                    // If there's a winner, we give '+' earnings. Else '-' earnings.
                    modifier = winner.Value ? '+' : '-';
                }

                bool nullMod = !modifier.HasValue;
                if(nullMod)
                    fSb.AppendLine($"Points Balance: {user.Points:N0}");
                else
                {
                    // If winner, we display "+(entire pot)"...else display "-(user points bet)".
                    string pointsVariance = winner.Value ? pokerData.pot.ToString("N0") : pokerData.userPointsBet.ToString("N0");
                    string changeStr = $"({modifier}{pointsVariance})"; //todo: Ensure proper values.
                    fSb.AppendLine($"Points Balance: {user.Points:N0} {changeStr}");
                }
            }
            
            if (showPot)
            {
                if(!lastTurn)
                    fSb.AppendLine(sb.Replace(",", " | ").ToString()[..^3]);

                return fSb.ToString();
            }
            
            return sb.ToString();
        }
        #endregion

        #region ReactionCallback Methods {...}
        private static ReactionCallbackData EmbedReactionData(EmbedBuilder embed, PokerData pokerData, int timeoutSeconds, bool canCheck, bool canCall, 
            bool canRaise, bool canFold, bool lastTurn, User user = null, int raise = 0, int initialBet = 0)
        {
            if(lastTurn)
                return new ReactionCallbackData("", embed.Build());
            
            var data = new ReactionCallbackData("", embed.Build(), true, true, TimeSpan.FromSeconds(timeoutSeconds));
            var callbacks = new List<(IEmote, Func<SocketCommandContext, SocketReaction, Task>)>();
            
            if(canCheck)
                callbacks.Add(CheckActionCallback());
            if (canCall)
            {
                if (user == null || raise == 0 || initialBet == 0)
                {
                    throw new InvalidOperationException("In order for there to be a Call in this poker game, " +
                                                        "the user, raise, and inital bet values must be set!!");
                }
                callbacks.Add(CallActionCallback(pokerData, user, raise, initialBet));
            }
            if (canRaise)
            {
                callbacks.Add(RaiseActionCallback());
            }
            if (canFold)
            {
                callbacks.Add(FoldActionCallback());
            }

            data.SetCallbacks(callbacks.ToArray());
            return data;
        }
        
        private static (IEmote, Func<SocketCommandContext, SocketReaction, Task>) CallActionCallback(PokerData pokerData, 
            User user, int amountToCall, int initialBet)
        {
            (IEmote, Func<SocketCommandContext, SocketReaction, Task>) callback = (PokerData.rcCall, async (c, r) =>
            {
                if (user.Points < amountToCall + initialBet)
                {
                    pokerData.userPointsBet += user.Points;
                    await c.Channel.SendMessageAsync($"{c.User.Mention} You do not have enough points to call. " +
                                                     $"You are now all in!");
                }
                
                pokerData.userPointsBet += amountToCall;
                await c.Channel.SendMessageAsync($"{c.User.Mention} You have decided to call the dealer's " +
                                                 $"raise of `{amountToCall:N0}` points.");
                
                PokerEvent.TurnTrigger(new PokerGameEventArgs(user, amountToCall, PokerGameAction.CALL));
            });

            return callback;
        }

        private static (IEmote, Func<SocketCommandContext, SocketReaction, Task>) CheckActionCallback()
        {
            (IEmote, Func<SocketCommandContext, SocketReaction, Task>) callback = (PokerData.rcCheck, async (c, r) =>
            {
                //todo: Create embeds that are prettier.

                PokerEvent.TurnTrigger(new PokerGameEventArgs(null, 0, PokerGameAction.CHECK));
            });

            return callback;
        }
        
        private static (IEmote, Func<SocketCommandContext, SocketReaction, Task>) RaiseActionCallback()
        {
            (IEmote, Func<SocketCommandContext, SocketReaction, Task>) callback = (PokerData.rcRaise, async (c, r) =>
            {
                //todo: Create embeds that are prettier.
                //todo: Gather user input for how much they want to raise.
                PokerEvent.TurnTrigger(new PokerGameEventArgs(null, 0, PokerGameAction.RAISE));
            });

            return callback;
        }
        
        private static (IEmote, Func<SocketCommandContext, SocketReaction, Task>) FoldActionCallback()
        {
            (IEmote, Func<SocketCommandContext, SocketReaction, Task>) callback = (PokerData.rcFold, async (c, r) =>
            {
                //todo: Create embeds that are prettier.

                await c.Channel.SendMessageAsync($"{c.User.Mention} You have decided to forfeit and fold your hand!");
                PokerEvent.TurnTrigger(new PokerGameEventArgs(null, 0, PokerGameAction.FOLD));
            });

            return callback;
        }

        #endregion

        public static Card[] GenerateSuit(IEmote emoji)
        {
            Card[] suit = new Card[SUIT_LEN];
            for (int i = 0; i < SUIT_LEN; i++)
            {
                string cardText = String.Empty;
                if (i == 0)
                {
                    cardText = "A";
                }
                else if (i >= 1 && i <= 9)
                {
                    cardText = (i + 1).ToString();
                }
                else
                {
                    switch (i)
                    {
                        case 10:
                            cardText = "J";
                            break;
                        case 11:
                            cardText = "Q";
                            break;
                        case 12:
                            cardText = "K";
                            break;
                        default:
                            throw new KaguyaSupportException("An unexpected error occured when " +
                                                             "generating a card. Please report this error.");
                    }
                }

                suit[i] = new Card(emoji, cardText);
            }

            return suit;
        }

        public static Card GenerateRandomCard()
        {
            Card[] suitCards;
            List<Card> invalidCards = PokerData.cardsDrawn;
            
            Random r = new Random();
            
            var cardGenException = new KaguyaSupportException("An unexpected error occurred when trying " +
                                                              "to generate a card. Please report this error.");
            Card card;
            do
            {
                int suitNum = r.Next(4);
                int cardNum = r.Next(13);

                switch (suitNum)
                {
                    case 0:
                        suitCards = PokerData.suitHearts;
                        break;
                    case 1:
                        suitCards = PokerData.suitDiamonds;
                        break;
                    case 2:
                        suitCards = PokerData.suitSpades;
                        break;
                    case 3:
                        suitCards = PokerData.suitClubs;
                        break;

                    default:
                        throw cardGenException;
                }

                card = suitCards[cardNum];
            } while (invalidCards.Contains(card));
            
            if (card.Equals(null))
                throw cardGenException;
            
            invalidCards.Add(card);
            PokerData.cardsDrawn = invalidCards;
            
            return card;
        }

        /// <summary>
        /// Generates a 2-card hand for a player.
        /// </summary>
        /// <param name="suitNum">A number that determines what suit the card will be in.
        /// 0 = hearts
        /// 1 = diamonds
        /// 2 = spades
        /// 3 = clubs</param>
        /// <returns></returns>
        private static Hand GeneratePlayerHand()
        {
            Card[] handCards = new Card[2];
            for (int i = 0; i < 2; i++)
                handCards[i] = GenerateRandomCard();

            return new Hand(handCards);
        }

        private static Card[] GenerateFlop(int length = 3)
        {
            Card[] flop = new Card[length];
            for (int i = 0; i < length; i++)
                flop[i] = GenerateRandomCard();

            return flop;
        }

        /// <summary>
        /// Returns whether or not the hand has any "of a kind" matching values in the communityCards.
        /// Example: 2 of a kind means the hand has 2 matches between itself and the community cards.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="communityCards"></param>
        /// <returns></returns>
        private static bool HasOfAKind(string[] hand, string[] communityCards)
        {
            foreach (var card in hand)
            {
                if (communityCards.Contains(card))
                    return true;
            }

            return false;
        }

        private static void RemoveGameFromCache(ulong userId)
        {
            MemoryCache.ActivePokerSessions.Remove(userId);
        }
    }

    public class PokerData
    {
        // This may be inefficient.
        private static readonly SocketGuild Guild = KaguyaBase.Client.GetGuild(546880579057221644);

        public static readonly Emote cardbackEmote = Guild.Emotes.FirstOrDefault(x => x.Id == 761212621676085278);
        
        public static readonly Emoji heartEmoji = new Emoji("♥️️");
        public static readonly Emoji diamondEmoji = new Emoji("♦️️️");
        public static readonly Emote spadeEmote = Guild.Emotes.FirstOrDefault(x => x.Id == 761234268743532564);
        public static readonly Emote clubEmote = Guild.Emotes.FirstOrDefault(x => x.Id == 761234268591489034);
        
        public static readonly Emoji rcFold = new Emoji("⛔");
        public static readonly Emoji rcRaise = new Emoji("⬆️");
        public static readonly Emoji rcCall = new Emoji("👍");
        public static readonly Emoji rcCheck = new Emoji("👌");
            
        public static readonly Card[] suitHearts = PokerGame.GenerateSuit(heartEmoji);
        public static readonly Card[] suitDiamonds = PokerGame.GenerateSuit(diamondEmoji);
        public static readonly Card[] suitSpades = PokerGame.GenerateSuit(spadeEmote);
        public static readonly Card[] suitClubs = PokerGame.GenerateSuit(clubEmote);

        public const double multiplier = 2.15;
        
        public int userPointsBet;
        public int dealerPointsBet;

        public double pot;

        public static List<Card> cardsDrawn = new List<Card>();

        public void UpdatePot() => pot = (userPointsBet * multiplier) + dealerPointsBet;

        public PokerData(int userPointsBet)
        {
            userPointsBet = userPointsBet;
            dealerPointsBet = 0;
            UpdatePot();
        }
        public static Card[] GetCardsForSuit(Suit suit)
        {
            switch (suit)
            {
                case Suit.HEARTS:
                    return suitHearts;
                case Suit.DIAMONDS:
                    return suitDiamonds;
                case Suit.SPADES:
                    return suitSpades;
                case Suit.CLUBS:
                    return suitClubs;
                default:
                    throw new KaguyaSupportException("An unexpected error occurred when trying to gather cards " +
                                                     $"for this particular suit: `{suit.Humanize()}`. Please report " +
                                                     $"this error in our support Discord.");
            }
        }

        /// <summary>
        /// Compares the player's hand to the community cards and determines an appropriate ranking.
        /// </summary>
        /// <param name="playerHand"></param>
        /// <param name="communityCards"></param>
        /// <returns></returns>
        public static HandRanking IdentifyRanking(Hand playerHand, Hand communityCards)
        {
            if (IsRoyalFlush(playerHand, communityCards))
                return HandRanking.ROYAL_FLUSH;

            if (IsStraight(playerHand, communityCards) && IsFlush(playerHand, communityCards))
                return HandRanking.STRAIGHT_FLUSH;

            if (IsOfKind(playerHand, communityCards, 4))
                return HandRanking.FOUR_OF_A_KIND;
            
            if(IsFullHouse(playerHand, communityCards))
                return HandRanking.FULL_HOUSE;

            if (IsFlush(playerHand, communityCards))
                return HandRanking.FLUSH;

            if (IsStraight(playerHand, communityCards))
                return HandRanking.STRAIGHT;

            if (IsOfKind(playerHand, communityCards, 3))
                return HandRanking.THREE_OF_A_KIND;

            if (IsPair(playerHand, communityCards, 2))
                return HandRanking.TWO_PAIR;

            if (IsPair(playerHand, communityCards, 1))
                return HandRanking.PAIR;

            return HandRanking.HIGH_CARD;
        }

        public static bool IsRoyalFlush(Hand playerHand, Hand communityCards)
        {
            if (!IsFlush(playerHand, communityCards))
                return false;
            
            return IsRoyalStraight(playerHand, communityCards);
        }

        public static bool IsFullHouse(Hand playerHand, Hand communityCards)
        {
            int matchCount = 0;
            foreach (var card in playerHand.Cards)
            {
                if (!communityCards.Cards.Contains(card))
                    return false;

                matchCount += communityCards.Cards.Count(x => x.NumericValue == card.NumericValue);
            }

            return matchCount == 5;
        }
        
        public static bool IsFlush(Hand playerHand, Hand communityCards)
        {
            var cardCollection = new List<Card>();
            cardCollection.AddRange(playerHand.Cards);
            cardCollection.AddRange(communityCards.Cards);

            // You need at least 5 cards for a Flush.
            if (cardCollection.Count < 5)
                return false;

            Suit mostCommonSuit = FindMostCommonSuit(cardCollection);
            if (cardCollection.Count(x => x.Suit == mostCommonSuit) < 5)
                return false; // False if there are not 5 cards of the same suit. True if there are.
            
            return true;
        }

        /// <summary>
        /// Determines whether or not a player's hand, when compared against the dealer's,
        /// has a "of a kind" match...i.e. 3 of a kind, 4 of a kind.
        /// </summary>
        /// <param name="playerHand"></param>
        /// <param name="communityCards"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool IsOfKind(Hand playerHand, Hand communityCards, int amount)
        {
            if(amount < 2 || amount > 4)
                throw new InvalidOperationException("This method is only used to determine whether " +
                                                    "a player's hand contains a 3 of a kind or 4 of a kind " +
                                                    "when compared against the community cards.");
            
            foreach (var card in playerHand.Cards)
            {
                int matchCount = playerHand.Cards.Count(x => x.ValueText == card.ValueText);
                int cMatches = communityCards.Cards.Count(x => x.ValueText == card.ValueText);

                if (matchCount + cMatches == amount)
                    return true;
            }

            return false;
        }

        public static bool IsPair(Hand playerHand, Hand communityHand, int amount)
        {
            int totalPairs = 0;
            foreach (var card in playerHand.Cards)
            {
                if (communityHand.Cards.Count(x => x.NumericValue == card.NumericValue) >= 1)
                {
                    totalPairs++;
                }
            }

            return totalPairs == amount;
        }

        public static bool PlayerHasHighestCard(Hand playerHand, Hand dealerHand)
        {
            if(playerHand.Cards.Length < 2 || dealerHand.Cards.Length < 2)
                throw new InvalidOperationException("Both the player's hand and the dealer's hand " +
                                                    "must have a length of at least 2 in order for the " +
                                                    "high card comparison to be executed.");
            
            int[] pOrdered = playerHand.Cards.OrderByDescending(x => x.NumericValue).Select(x => x.NumericValue).ToArray();
            int[] dOrdered = dealerHand.Cards.OrderByDescending(x => x.NumericValue).Select(x => x.NumericValue).ToArray();

            int pFirst = pOrdered[0];
            int dFirst = dOrdered[0];

            if (pFirst == dFirst)
            {
                int pSecond = pOrdered[1];
                int dSecond = dOrdered[1];

                return pSecond > dSecond;
            }

            return pFirst > dFirst;
        }

        public static Suit FindMostCommonSuit(IEnumerable<Card> cards)
        {
            Suit mostCommonSuit = cards.OrderByDescending(x => x.Suit)
                .GroupBy(x => x.Suit)
                .Select(grp => grp.Key).First();
            return mostCommonSuit;
        }

        /// <summary>
        /// Returns whether or not the collection of cards provided is a 'royal straight' - a striaght consisting
        /// of a 10, Jack, Queen, King, and Ace only. Suit is not taken into account here.
        /// </summary>
        /// <param name="playerHand"></param>
        /// <param name="communityCards"></param>
        /// <returns></returns>
        /// <exception cref="PokerStraightException">Thrown if more than 5 cards are passed into the function.</exception>
        public static bool IsRoyalStraight(Hand playerHand, Hand communityCards)
        {
            var collection = new List<Card>();
            collection.AddRange(playerHand.Cards);
            collection.AddRange(communityCards.Cards);
            
            if (!IsStraight(playerHand, communityCards))
                return false;

            return collection.Count(x => x.NumericValue >= 10) == 5;
        }

        public static bool IsStraight(Hand playerHand, Hand communityCards)
        {
            var pHandCopy = playerHand.Cards.OrderByDescending(x => x.NumericValue).ToArray();
            var cHandCopy = communityCards.Cards.OrderByDescending(x => x.NumericValue).ToArray();

            foreach (var card in pHandCopy)
            {
                int deltaSum = 0;
                int offset = 0;
                foreach (var card2 in cHandCopy)
                {
                    int deltaAbs = Math.Abs(card.NumericValue - card2.NumericValue);

                    if (deltaAbs - offset == 1)
                    {
                        deltaSum++;
                        offset++;
                    }
                    else
                        deltaSum = 0;

                    if (deltaSum == 5)
                        return true;
                }
            }

            return false;
        }
    }

    public class Hand
    {
        public Card[] Cards { get; set; }
        public int Length => Cards.Length;
        public Hand(IEnumerable<Card> hand)
        {
            this.Cards = hand.ToArray();
        }
        
        public override string ToString()
        {
            return Cards.Humanize(x => x, "").Replace(",", "");
        }

        /// <summary>
        /// Adds a card to this <see cref="Hand"/>'s <see cref="Cards"/>.
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(Card card)
        {
            List<Card> newHandCards = Cards.ToList();

            newHandCards.Add(card);
            this.Cards = newHandCards.ToArray();
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            var newHandCards = Cards.ToList();
            
            newHandCards.AddRange(cards);
            this.Cards = newHandCards.ToArray();
        }
    }

    public class Card
    {
        public IEmote SuitEmote { get; }
        /// <summary>
        /// The written value of the card. Ex: "A", "J", "K", "5", etc.
        /// </summary>
        public string ValueText { get; }

        public Suit Suit
        {
            get
            {
                if (SuitEmote.Equals(PokerData.heartEmoji))
                    return Suit.HEARTS;
                if (SuitEmote.Equals(PokerData.diamondEmoji))
                    return Suit.DIAMONDS;
                if (SuitEmote.Equals(PokerData.spadeEmote))
                    return Suit.SPADES;
                if (SuitEmote.Equals(PokerData.clubEmote))
                    return Suit.CLUBS;
                
                throw new KaguyaSupportException("An unexpected error occurred when trying to generate the suit enum " +
                                                 "for a card. Please report this error.");
            }
        }

        /// <summary>
        /// Returns the numeric value of the card, range 1-10 inclusive. Aces are 1, face cards are 10.
        /// </summary>
        /// <exception cref="KaguyaSupportException"></exception>
        public int NumericValue
        {
            get
            {
                if (int.TryParse(ValueText, out int num))
                {
                    return num; // If the card is a 2-10, return it's value. Otherwise, calculate other value.
                }

                switch (ValueText)
                {
                    case "J":
                        return 11;
                    case "Q":
                        return 12;
                    case "K":
                        return 13;
                    case "A":
                        return 14;
                    default:
                        throw new KaguyaSupportException("An unexpected error occurred when determining the numeric " +
                                                         "value for a card.");
                }
            }
        }
        
        /// <summary>
        /// Gathers the card's numeric value *with account for the suit.*
        /// This range is 1-52 inclusive.
        /// </summary>
        /// <exception cref="KaguyaSupportException"></exception>
        public int NumericValueWithSuit
        {
            get
            {
                int suitNum = (int) Suit;
                return (suitNum * 13) + NumericValue;
            }
        }
        
        public bool IsFaceCard
        {
            get
            {
                string v = this.ValueText;
                return v == "J" || v == "Q" || v == "K";
            }
        }

        public bool IsAce => this.ValueText == "A";
        
        public override string ToString()
        {
            return SuitEmote + ValueText;
        }
        
        public Card(IEmote suitEmote, string value)
        {
            this.SuitEmote = suitEmote;
            this.ValueText = value;
        }
    }

    public enum HandRanking
    {
        ROYAL_FLUSH,
        STRAIGHT_FLUSH,
        FOUR_OF_A_KIND,
        FULL_HOUSE,
        FLUSH,
        STRAIGHT,
        THREE_OF_A_KIND,
        TWO_PAIR,
        PAIR,
        HIGH_CARD
    }

    public enum Suit
    {
        HEARTS,
        DIAMONDS,
        SPADES,
        CLUBS
    }

    public enum PokerGameAction
    {
        CHECK,
        CALL,
        RAISE,
        FOLD
    }
}