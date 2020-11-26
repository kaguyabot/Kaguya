using System;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaguya.Database.Repositories
{
	public class CommandHistoryRepository : ICommandHistoryRepository
	{
		private readonly KaguyaDbContext _dbContext;
		private readonly ILogger<CommandHistoryRepository> _logger;
		
		public CommandHistoryRepository(KaguyaDbContext dbContext, ILogger<CommandHistoryRepository> logger)
		{
			_logger = logger;
			_dbContext = dbContext;
		}
		
		public async Task<CommandHistory> GetAsync(ulong userId, ulong serverId, string command)
		{
			return await _dbContext.CommandHistories
			                       .AsQueryable()
			                       .FirstOrDefaultAsync(x => x.UserId == userId && 
			                                                 x.ServerId == serverId && 
			                                                 x.CommandName.Equals(command, StringComparison.OrdinalIgnoreCase));
		}

		// TODO: Async naming?
		public Task UpdateAsync(CommandHistory value)
		{
			_dbContext.CommandHistories.Update(value);
			return Task.CompletedTask;
		}

		public Task DeleteAsync(CommandHistory value)
		{
			_dbContext.CommandHistories.Remove(value);
			return Task.CompletedTask;
		}

		public Task InsertAsync(CommandHistory value)
		{
			_dbContext.CommandHistories.Add(value);

			return Task.CompletedTask;
		}
	}
}