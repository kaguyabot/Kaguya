using System;
using System.Threading.Tasks;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Currency.Poker
{
    public static class PokerEvent
    {
        public static event Func<PokerGameEventArgs, Task> OnTurnEnd;

        public static void TurnTrigger(PokerGameEventArgs e)
        {
            OnTurnEnd?.Invoke(e);
        }
        
    }
    public class PokerGameEventArgs : EventArgs
    {
        public User User { get; }
        public int RaisePoints { get; }
        public PokerGameAction Action { get; }

        public PokerGameEventArgs(User user, int raisePoints, PokerGameAction action)
        {
            this.User = user;
            this.RaisePoints = raisePoints;
            this.Action = action;
        }
    }
}