using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency
{
    public class PokerGame : KaguyaBase
    {
        private const int SUIT_LEN = 13;
        
        [CurrencyCommand]
        [Command("Poker")]
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
            var user = await DatabaseQueries.GetOrCreateUserAsync(Context.User.Id);
            var cardBackEmote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 761212621676085278);

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

            double multiplier = 2.2;
            double pot = points * multiplier;
            Hand playerHand = GeneratePlayerHand();
            Hand dealerHand = GeneratePlayerHand();
            
            List<Card> communityCards = GenerateFlop(5).ToList(); // We start with a flop, then add onto this later.
            Hand communityCardsAsHand = new Hand(communityCards);

            HandRanking playerHandRanking = PokerData.IdentifyRanking(playerHand, communityCardsAsHand);
            HandRanking dealerHandRanking = PokerData.IdentifyRanking(dealerHand, communityCardsAsHand);
            
            KaguyaEmbedBuilder embed = GameEmbed(playerHand, communityCards, (int)pot);
            await SendEmbedAsync(embed);
        }

        private static KaguyaEmbedBuilder GameEmbed(Hand playerHand, IEnumerable<Card> communityCards, int pot)
        {
            var embed = new KaguyaEmbedBuilder(EmbedColor.MAGENTA)
            {
                Title = "Kaguya Poker",
                Fields = new List<EmbedFieldBuilder>
                {
                    // We hide the dealer's hand from view.
                    new EmbedFieldBuilder
                    {
                        Name = "Dealer Hand",
                        Value = $"{PokerData.cardbackEmote}? {PokerData.cardbackEmote}?",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Player Hand",
                        Value = playerHand.ToString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Table Cards",
                        Value = communityCards.ToReadable(),
                        IsInline = false
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Current Pot: {pot:N0}"
                }
            };
            return embed;
        }

        private static (IEmote, Func<SocketCommandContext, SocketReaction, Task>) CallActionCallback(User user, 
            int raise, double multiplier, int pot)
        {
            (IEmote, Func<SocketCommandContext, SocketReaction, Task>) callback = (PokerData.rcCall, async (c, r) =>
            {
                //todo: Come back and make a callback item for a 'call' poker event.
            });

            return callback;
        }

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

            Random r = new Random();
            int suitNum = r.Next(4);
            int cardNum = r.Next(13);
            
            var cardGenException = new KaguyaSupportException("An unexpected error occurred when trying " +
                                                              "to generate a card. Please report this error.");
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

            var card = suitCards[cardNum];

            if (card.Equals(null))
                throw cardGenException;
            
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
    }

    struct PokerData
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
            if (HasRoyalFlush(playerHand, communityCards))
                return HandRanking.ROYAL_FLUSH;
            return HandRanking.PAIR;
        }

        public static bool HasRoyalFlush(Hand playerHand, Hand communityCards)
        {
            if (!HasFlush(playerHand, communityCards))
                return false;
            
            //Determining 'royal straight'
            var cardCollection = new List<Card>();
            cardCollection.AddRange(playerHand.HandCards);
            cardCollection.AddRange(communityCards.HandCards);

            Suit mostCommonSuit = FindMostCommonSuit(cardCollection);
            List<Card> royalStraightCheck = new List<Card>();
            
            royalStraightCheck.Add(cardCollection[0]);
            royalStraightCheck.AddRange(cardCollection.ToArray()[^4..]);

            return IsRoyalStraight(royalStraightCheck);
        }

        public static bool HasFlush(Hand playerHand, Hand communityCards)
        {
            var cardCollection = new List<Card>();
            cardCollection.AddRange(playerHand.HandCards);
            cardCollection.AddRange(communityCards.HandCards);

            // You need at least 5 cards for a Flush.
            if (cardCollection.Count < 5)
                return false;

            Suit mostCommonSuit = FindMostCommonSuit(cardCollection);
            if (cardCollection.Count(x => x.Suit == mostCommonSuit) < 5)
                return false; // False if there are not 5 cards of the same suit. True if there are.
            
            return true;
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
        /// <param name="cards">The collection of cards to check for whether a straight exists.</param>
        /// <returns></returns>
        /// <exception cref="PokerStraightException">Thrown if more than 5 cards are passed into the function.</exception>
        public static bool IsRoyalStraight(IEnumerable<Card> cards)
        {
            Card[] collection = cards as Card[] ?? cards.ToArray();
            if (!IsStraight(collection))
                return false;

            return collection.Count(x => x.NumericValue == 10) + collection.Count(x => x.NumericValue == 1) == 5;
        }

        public static bool IsStraight(IEnumerable<Card> cards)
        {
            Card[] collection = cards as Card[] ?? cards.ToArray();
            if(collection.Length != 5)
                throw new PokerStraightException("There must be exactly 5 cards to check against when determining " +
                                                 "whether a collection is a straight!");
            
            var numericCollection = collection.OrderByDescending(x => x.NumericValue).ToArray();
            
            // A delta of 5 here means the collection is a straight.
            // A delta of 9 here means the collection is a 'royal straight' (Face card [10] - Ace [1] = 9)
            int delta = numericCollection[0].NumericValue - numericCollection[4].NumericValue;
            return delta == 5 || delta == 9;
        }
    }

    public class Hand
    {
        public Card[] HandCards { get; set; }
        public int Length => HandCards.Length;
        public Hand(IEnumerable<Card> hand)
        {
            this.HandCards = hand.ToArray();
        }
        
        public override string ToString()
        {
            return HandCards.Humanize(x => x, "").Replace(",", "");
        }

        /// <summary>
        /// Adds a card to this <see cref="Hand"/>'s <see cref="HandCards"/>.
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(Card card)
        {
            Card[] newHandCards = { };
            Array.Copy(HandCards, newHandCards, HandCards.Length + 1);

            newHandCards[^1] = card;
            this.HandCards = newHandCards;
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
                    case "A":
                        return 1;
                    case "J": case "Q": case "K":
                        return 10;
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

    enum HandRanking
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
}