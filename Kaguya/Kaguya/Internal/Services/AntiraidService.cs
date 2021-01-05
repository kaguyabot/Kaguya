using System;
using System.Threading.Tasks;
using Kaguya.Internal.Events.ArgModels;

namespace Kaguya.Internal.Services
{
    public class AntiraidService : ITimerReceiver
    {
        public Task HandleTimer(object payload)
        {
            if (payload is not AntiraidEventArgs eventArgs)
            {
                throw new InvalidCastException($"The type received was {payload.GetType().Name}. Expected " +
                                               $"{nameof(AntiraidEventArgs)}.");
            }
            
            return Task.CompletedTask;
        }
    }
}