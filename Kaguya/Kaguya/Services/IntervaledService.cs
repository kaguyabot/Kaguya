using System.Threading.Tasks;
using System.Timers;

namespace Kaguya.Services
{
    public abstract class IntervaledService
    {
        protected readonly Timer Timer;
        
        protected IntervaledService(long milliseconds)
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