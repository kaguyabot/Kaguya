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
        
        private static int _pointsToDeductFromUser = 0;
        
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
            //todo: When launching a second poker game after one has completed, 2x messages are sent.

            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var cardBackEmote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 761212621676085278);

            if (MemoryCache.ActivePokerSessions.Contains(Context.User.Id))
            {
                await SendBasicErrorEmbedAsync($"{Context.User.Mention} You are already in a poker game. " +
                                               $"Finish the first one to play again!");
                return;
            }
            MemoryCache.ActivePokerSessions.Add(user.UserId);
            
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

            PokerData.pot = points * PokerData.multiplier;
            Hand playerHand = GeneratePlayerHand();
            Hand dealerHand = GeneratePlayerHand();
            
            List<Card> communityCards = GenerateFlop().ToList(); // We start with a flop, then add onto this later.
            Hand communityHand = new Hand(communityCards);

            // Turn 1 - display basic information to user (their hand + the flop)
            var embed = OverviewEmbed(playerHand, dealerHand, communityHand, true, false, true, true, false);
            var data = EmbedReactionData(embed, TIMEOUT, true, false, true, true, false);
            await InlineReactionReplyAsync(data);

            // Subsequent turns
            Func<PokerGameEventArgs, Task> pokerEventHandler = PokerTurnHandler(playerHand, dealerHand, communityHand);
            PokerEvent.OnTurnEnd += pokerEventHandler;
            PokerEvent.OnGameFinished += () => Task.Run(() => PokerEvent.OnTurnEnd -= pokerEventHandler);
        }

        private Func<PokerGameEventArgs, Task> PokerTurnHandler(Hand playerHand, Hand dealerHand, Hand communityHand)
        {
            Func<PokerGameEventArgs, Task> pokerEventHandler = async e =>
            {
                bool lastTurn = communityHand.Length == 5;

                if (!lastTurn && e.Action == PokerGameAction.FOLD)
                    lastTurn = true;

                if (!lastTurn)
                    communityHand.AddCard(GenerateRandomCard());

                switch (e.Action)
                {
                    case PokerGameAction.CHECK:
                        if (lastTurn)
                            break;

                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} You have decided to check.");

                        //todo: If last turn, don't include footer.
                        var checkEmbed = ResponseEmbed(playerHand, dealerHand, communityHand, lastTurn, e.Action);
                        var callData = EmbedReactionData(checkEmbed, TIMEOUT, true, false, true, true, lastTurn);
                        await InlineReactionReplyAsync(callData);
                        break;
                }

                if (lastTurn)
                {
                    MemoryCache.ActivePokerSessions.Remove(Context.User.Id);

                    HandRanking playerRanking = PokerData.IdentifyRanking(playerHand, communityHand);
                    HandRanking dealerRanking = PokerData.IdentifyRanking(dealerHand, communityHand);

                    bool playerWinner = playerRanking > dealerRanking;
                    bool tie = playerRanking == dealerRanking;
                    
                    PokerEvent.GameFinishedTrigger();

                    if (tie)
                    {
                        if (playerRanking == HandRanking.HIGH_CARD && dealerRanking == HandRanking.HIGH_CARD)
                        {
                            playerWinner = PlayerIsHighCardWinner(playerHand, dealerHand);
                        }
                    }

                    if (!playerWinner && !tie) // The player wins...
                    {
                        await SendEmbedAsync(PlayerWinnerEmbed(playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking));
                    }
                    else if (playerRanking == dealerRanking)
                    {
                        await SendEmbedAsync(TieEmbed(playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking));
                    }
                    else
                    {
                        await SendEmbedAsync(LoseEmbed(playerHand, dealerHand, communityHand, playerRanking,
                            dealerRanking));
                    }
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
            HandRanking playerHandRanking, HandRanking dealerHandRanking)
        {
            return new KaguyaEmbedBuilder(EmbedColor.GREEN)
            {
                Title = "Kaguya Poker: Winner!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(false, false, false, false, true) + "\n" +
                           "" +
                           $"Your hand: {playerHandRanking.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHandRanking.Humanize(LetterCasing.Title)}"
                }
            };
        }
        
        // Yes...this doesn't happen in real life, but for simplicity, we will implement the possibility of a tie.
        private static KaguyaEmbedBuilder TieEmbed(Hand playerHand, Hand dealerHand, Hand communityHand,
            HandRanking playerHr, HandRanking dealerHr)
        {
            return new KaguyaEmbedBuilder(EmbedColor.GOLD)
            {
                Title = "Kaguya Poker: Tie!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(false, false, false, false, true) + "\n" +
                           $"Your hand: {playerHr.Humanize(LetterCasing.Title)}\n" +
                           $"Dealer's hand: {dealerHr.Humanize(LetterCasing.Title)}"
                }
            };
        }
        
        private static KaguyaEmbedBuilder LoseEmbed(Hand playerHand, Hand dealerHand, Hand communityHand,
            HandRanking playerHr, HandRanking dealerHr)
        {
            return new KaguyaEmbedBuilder(EmbedColor.LIGHT_PURPLE)
            {
                Title = "Kaguya Poker: Loser!",
                Fields = PokerTableEmbedFields(playerHand, dealerHand, communityHand, true),
                Footer = new EmbedFooterBuilder
                {
                    Text = GetOptionsFooter(false, false, false, false, true) + "\n" +
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

        private static string GetOptionsFooter(bool canCheck, bool canCall, bool canRaise, bool canFold, bool lastTurn)
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
            if(!lastTurn)
                fSb.AppendLine(sb.Replace(",", " | ").ToString()[..^3]);

            return fSb.ToString();
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
                    PokerData.pot += user.Points;
                    _pointsToDeductFromUser = user.Points;
                    await c.Channel.SendMessageAsync($"{c.User.Mention} You do not have enough points to call. " +
                                                             $"You are now all in!");
                }
                
                PokerData.pot += amountToCall;
                _pointsToDeductFromUser += amountToCall;

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

        /// <summary>
        /// Returns true if the player's hand is better than the dealer's.
        /// </summary>
        /// <param name="playerHand"></param>
        /// <param name="dealerHand"></param>
        /// <returns></returns>
        private static bool PlayerIsHighCardWinner(Hand playerHand, Hand dealerHand)
        {
            int pVal = playerHand.Cards.Sum(x => x.NumericValue);
            int dVal = dealerHand.Cards.Sum(x => x.NumericValue);
            return pVal > dVal;
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

        public static double multiplier = 2.2;
        public static double pot = 0;

        public static int userPointsBet = 0;
        public static int dealerPointsBet = 0;

        public static List<Card> cardsDrawn = new List<Card>();

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

        public static bool IsOfKind(Hand playerHand, Hand communityCards, int amount)
        {
            int pairCount = 0;
            foreach (var card in playerHand.Cards)
            {
                if (card.IsFaceCard || card.IsAce)
                {
                    if (communityCards.Cards.Any(x => x.ValueText == card.ValueText))
                    {
                        pairCount++;
                        break;
                    }
                }
                else
                {
                    if (communityCards.Cards.Any(x => x.NumericValue == card.NumericValue))
                    {
                        pairCount++;
                        break;
                    }
                }
            }

            return pairCount == amount;
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

        public static int HighestCard(Hand playerHand)
        {
            return playerHand.Cards.OrderByDescending(x => x.NumericValue).First().NumericValue;
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
        /// <param name="cards">The collection of cards to check for whether a straight exists.</param>
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
            playerHand.Cards = playerHand.Cards.OrderByDescending(x => x.NumericValue).ToArray();
            communityCards.Cards = communityCards.Cards.OrderByDescending(x => x.NumericValue).ToArray();

            foreach (var card in playerHand.Cards)
            {
                int deltaSum = 0;
                foreach (var card2 in communityCards.Cards)
                {
                    int deltaAbs = Math.Abs(card.NumericValue - card2.NumericValue);

                    if (deltaAbs == 1)
                        deltaSum++;
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