using Kaguya.Database.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
	public interface IReminderRepository : IRepository<Reminder>
	{
		public Task<IList<Reminder>> GetAllAsync(ulong id);
		public Task<IList<Reminder>> GetAllToDeliverAsync(ulong id);

		/// <summary>
		///  All reminders in which the expiration time has passed but the reminder
		///  has not been delivered to the user.
		/// </summary>
		/// <returns></returns>
		public Task<IList<Reminder>> GetAllToDeliverAsync();
	}
}