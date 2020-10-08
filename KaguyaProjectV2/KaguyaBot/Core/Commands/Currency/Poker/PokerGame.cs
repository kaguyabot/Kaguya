using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using KaguyaProjectV2.KaguyaBot.Core.Extensions.DiscordExtensions;
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
                 "Rules:\n" +
                 "- In order to play, you must bet at least 500 points, up to 50,000 points. The dealer will also match " +
                 "your bet, so if you win, you will win `2.2x` whatever you put into the pot.\n" +
                 "- Each player (you and the dealer) will be dealt two cards. You will not know what the dealer's hand is.\n" +
                 "- After the hands are dealt, there will be a `flop` of 3 random cards. At this point, you can either " +
                 "`fold`, `raise`, `call`, or `check` if you were raised. " +
                 "Be careful - if the dealer thinks they have a good hand, they will raise you! " +
                 "If you fail to `call` the dealer's raise, you forfeit your winnings and automatically fold.\n" +
                 "- After you have `called` or `checked`, the next round of betting will begin with a new card displayed: " +
                 "the `turn card`. The same options will be displayed again.\n" +
                 "- Finally, the `river card` will be displayed. This is the last round of betting.")]
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
            
            // We must only add to the cache after the intial serialization has occurred.
            // Otherwise, the user will be unable to start a new Poker game.
            MemoryCache.ActivePokerSessions.Add(user.UserId);

            // We don't set PokerData.dealerPointsBet yet because the pot already multiplies the user's bet 
            // by the multiplier. That value should only be set when the dealer AI makes a decision to bet/raise.
            PokerData.userPointsBet += points;
            PokerData.UpdatePot();

            Hand playerHand = GeneratePlayerHand();
            Hand dealerHand = GeneratePlayerHand();
            
            List<Card> communityCards = GenerateFlop().ToList(); // We start with a flop, then add onto this later.
            Hand communityHand = new Hand(communityCards);

            // Turn 1 - display basic information to user (their hand + the flop)
            var embed = OverviewEmbed(playerHand, dealerHand, communityHand, true, false, true, true, false);
            var data = EmbedReactionData(embed, TIMEOUT, true, false, true, true, false);
            await InlineReactionReplyAsync(data);

            // Subsequent turns
            Func<PokerGameEventArgs, Task> pokerEventHandler = PokerTurnHandler(playerHand, dealerHand, communityHand, user);
            PokerEvent.OnTurnEnd += pokerEventHandler;
            PokerEvent.OnGameFinished += () => Task.Run(() => PokerEvent.OnTurnEnd -= pokerEventHandler);
        }

        private Func<PokerGameEventArgs, Task> PokerTurnHandler(Hand playerHand, Hand dealerHand, Hand communityHand,
            User user)
        {
            Func<PokerGameEventArgs, Task> pokerEventHandler = async e =>
            {
                bool lastTurn = communityHand.Length == 5;

                if (!lastTurn && e.Action == PokerGameAction.FOLD)
                    lastTurn = true;

                if (!lastTurn)
                    communityHand.AddCard(GenerateRandomCard());

                PokerData.UpdatePot();

                switch (e.Action)
                {
                    case PokerGameAction.CHECK:
                        if (lastTurn)
                            break;

                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} You have decided to check.");

                        //todo: If last turn, don't include footer.
                        var checkEmbed = ResponseEmbed(playerHand, dealerHand, communityHand, false, e.Action);
                        var callData = EmbedReactionData(checkEmbed, TIMEOUT, true, false, true, true, false);
                        await InlineReactionReplyAsync(callData);
                        break;
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

                    // todo: Determine logic for a "push" scenario, such as two flush draws, two straight draws, etc.
                    if (playerWinner)
                    {
                        await SendEmbedAsync(PlayerWinnerEmbed(playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking, user));
                    }
                    else if (push)
                    {
                        await SendEmbedAsync(TieEmbed(playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking, user));
                    }
                    else
                    {
                        await SendEmbedAsync(LoseEmbed(playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking, user));
                    }

                    PokerEvent.GameFinishedTrigger();
                }
            };
            return pokerEventHandler;
        }

        #region Embed helper methods {...}
        private static KaguyaEmbedBuilder OverviewEmbed(Hand playerHand, Hand dealerHand, Hand communityHand,
            bool canCheck, bool canCall, bool canRaise, bool canFold, bool lastTurn)
        {
            var embed = new KaguyaEmbedBuilder(POKER_COLOR)
            {
                Title = "Kaguya Poker",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, lastTurn),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(canCheck, canCall, canRaise, canFold, lastTurn)
                }
            };
            return embed;
        }

        private static KaguyaEmbedBuilder PlayerWinnerEmbed(Hand playerHand, Hand dealerHand, Hand communityHand, 
            HandRanking playerHandRanking, HandRanking dealerHandRanking, User user)
        {
            return new KaguyaEmbedBuilder(EmbedColor.GREEN)
            {
                Title = "Kaguya Poker: Winner!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(false, false, false, false, true, false) + "\n" +
                           $"Your hand: {playerHandRanking.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHandRanking.Humanize(LetterCasing.Title)}"
                }
            };
        }
        
        // Yes...this doesn't happen in real life, but for simplicity, we will implement the possibility of a tie.
        private static KaguyaEmbedBuilder TieEmbed(Hand playerHand, Hand dealerHand, Hand communityHand,
            HandRanking playerHr, HandRanking dealerHr, User user)
        {
            return new KaguyaEmbedBuilder(EmbedColor.GOLD)
            {
                Title = "Kaguya Poker: Tie!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(false, false, false, false, true, true, true, user) + "\n" +
                           $"Your hand: {playerHr.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHr.Humanize(LetterCasing.Title)}"
                }
            };
        }
        
        private static KaguyaEmbedBuilder LoseEmbed(Hand playerHand, Hand dealerHand, Hand communityHand,
            HandRanking playerHr, HandRanking dealerHr, User user)
        {
            return new KaguyaEmbedBuilder(EmbedColor.LIGHT_PURPLE)
            {
                Title = "Kaguya Poker: Loser!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(false, false, false, false, true, true, true, user) + "\n" +
                           $"Your hand: {playerHr.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHr.Humanize(LetterCasing.Title)}"
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
        private static KaguyaEmbedBuilder ResponseEmbed(Hand playerHand, Hand dealerHand, 
            Hand communityHand, bool lastTurn, PokerGameAction action)
        {
            string actionStr = action.Humanize(LetterCasing.Sentence);
            var embed = new KaguyaEmbedBuilder(POKER_COLOR)
            {
                Title = $"Kaguya Poker: {actionStr}",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, lastTurn),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(true, false, true, true, lastTurn)
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

        private static string GetOptionsFooter(bool canCheck, bool canCall, bool canRaise, bool canFold, bool lastTurn, 
            bool showPot = true, bool showUserPoints = false, User user = null)
        {
            string potStr = $"Current Pot: {(int) PokerData.pot:N0}\n";
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

                string? modifier = PokerData.userWinnings == 0 
                    ? string.Empty : PokerData.userWinnings < 0 
                        ? "-" : "+";

                string? changeStr = string.IsNullOrEmpty(modifier) ? null : $"({modifier}{PokerData.userWinnings:N0})";
                fSb.AppendLine($"Points Balance: {user.Points:N0} {changeStr}");
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

        private static ReactionCallbackData EmbedReactionData(EmbedBuilder embed, int timeoutSeconds, bool canCheck, bool canCall, 
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
                callbacks.Add(CallActionCallback(user, raise, initialBet));
            }
            if (canRaise)
            {
                callbacks.Add(RaiseActionCallback(raise));
            }
            if (canFold)
            {
                callbacks.Add(FoldActionCallback());
            }

            data.SetCallbacks(callbacks.ToArray());
            return data;
        }
        
        private static (IEmote, Func<SocketCommandContext, SocketReaction, Task>) CallActionCallback(User user, 
            int amountToCall, int initialBet)
        {
            (IEmote, Func<SocketCommandContext, SocketReaction, Task>) callback = (PokerData.rcCall, async (c, r) =>
            {
                if (user.Points < amountToCall + initialBet)
                {
                    PokerData.userPointsBet += user.Points;
                    await c.Channel.SendMessageAsync($"{c.User.Mention} You do not have enough points to call. " +
                                                     $"You are now all in!");
                }
                
                PokerData.userPointsBet += amountToCall;
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
        
        private static (IEmote, Func<SocketCommandContext, SocketReaction, Task>) RaiseActionCallback(int amount)
        {
            (IEmote, Func<SocketCommandContext, SocketReaction, Task>) callback = (PokerData.rcRaise, async (c, r) =>
            {
                //todo: Create embeds that are prettier.
                //todo: Gather user input for how much they want to raise.
                await c.Channel.SendMessageAsync($"{c.User.Mention} You have decided to raise the pot!");
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

    public struct PokerData
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

        public const double multiplier = 2.2;
        
        public static int userPointsBet;
        public static int dealerPointsBet;

        public static double pot;

        public static readonly double userWinnings = pot - userPointsBet;

        public static List<Card> cardsDrawn = new List<Card>();

        public static void UpdatePot() => pot = (userPointsBet * multiplier) + dealerPointsBet;
        
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

            // todo: Double check if IsStraight() should in-fact take
            // todo: in just a collection of cards (or player/community hands instead).
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

            if (IsOfKind(playerHand, communityCards, 3)) //todo: Again, we must take the player's hand into account.
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
        // todo: Test
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

        public static int FindMostCommonNumericValue(IEnumerable<Card> cards)
        {
            int mostCommonNum = cards.OrderByDescending(x => x.NumericValue)
                .GroupBy(x => x.NumericValue)
                .Select(grp => grp.Key).First();
            return mostCommonNum; //todo: Test
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
                int suitNum = (int) Suit; //todo: Ensure (int)Suit is range 0-3 inclusive.
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
        HIGH_CARD //todo: research?
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