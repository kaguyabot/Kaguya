using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.Handlers.WarnEvent;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent
{
    public static class FishEvent
    {
        public static event EventHandler<FishHandlerEventArgs> OnFish;

        public static void Trigger(User user, Fish fish, ICommandContext context)
        {
            FishEventTrigger(new FishHandlerEventArgs(user, fish, context));
        }

        static void FishEventTrigger(FishHandlerEventArgs e)
        {
            OnFish?.Invoke(null, e);
        }
    }

    public class FishHandlerEventArgs : EventArgs
    {
        public User User { get; }
        public Fish Fish { get; }
        public ICommandContext Context { get; }
        public FishHandlerEventArgs(User user, Fish fish, ICommandContext context)
        {
            this.User = user;
            this.Fish = fish;
            this.Context = context;
        }
    }

}
