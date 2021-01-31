using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class AutoAssignedRoleRepository : IAutoAssignedRoleRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public AutoAssignedRoleRepository(KaguyaDbContext dbContext) { _dbContext = dbContext; }
        
        public async Task<AutoAssignedRole> GetAsync(ulong serverId, ulong roleId)
        {
            return await _dbContext.AutoAssignedRoles
                                   .AsQueryable()
                                   .Where(x => x.ServerId == serverId && x.RoleId == roleId)
                                   .FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(ulong serverId, ulong roleId)
        {
            var match = await GetAsync(serverId, roleId);

            if (match != null)
            {
                _dbContext.AutoAssignedRoles.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(AutoAssignedRole value)
        {
            _dbContext.AutoAssignedRoles.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(AutoAssignedRole value)
        {
            _dbContext.AutoAssignedRoles.Add(value);
            await _dbContext.SaveChangesAsync();
        }
    }
}