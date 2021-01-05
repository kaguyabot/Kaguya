using System.Threading.Tasks;

namespace Kaguya.Internal.Services
{
    public class AntiraidService : ITimerReceiver
    {
        public Task HandleTimer(object payload)
        {
            return Task.CompletedTask;
        }
        
        
    }
}