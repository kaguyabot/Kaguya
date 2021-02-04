using System.Collections.Generic;
using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IReminderRepository : IRepository<Reminder>
    {
        public Task<IList<Reminder>> GetAllForUserAsync(ulong id);
        public Task<IList<Reminder>> GetAllToDeliverForUserAsync(ulong id);
        
        /// <summary>
        /// All reminders in which the expiration time has passed but the reminder
        /// has not been delivered to the user.
        /// </summary>
        /// <returns></returns>
        public Task<IList<Reminder>> GetAllToDeliverAsync();
    }
}