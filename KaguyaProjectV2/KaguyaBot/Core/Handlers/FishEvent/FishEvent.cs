using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.FishEvent
{
    public static class FishEvent
    {
        public static event Func<FishHandlerEventArgs, Task> OnFish;

        public static async Task Trigger(User user, Fish fish, ICommandContext context)
        {
            await FishEventTrigger(new FishHandlerEventArgs(user, fish, context));
        }

        static async Task FishEventTrigger(FishHandlerEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            await OnFish?.Invoke(e);
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
