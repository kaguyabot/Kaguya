using System.Threading.Tasks;
using System.Timers;

namespace Kaguya.Services
{
    public abstract class TimedKaguyaService
    {
        protected readonly Timer Timer;
        
        protected TimedKaguyaService(long milliseconds)
        {
            Timer = new Timer(milliseconds)
            {
                Enabled = true,
                AutoReset = true
            };

            Timer.Elapsed += (_, _) => OnElapsed();
        }

        protected abstract Task OnElapsed();
    }
}