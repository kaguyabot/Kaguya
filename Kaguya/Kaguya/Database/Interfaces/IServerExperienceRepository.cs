using System.Collections.Generic;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IServerExperienceRepository : IRepository<ulong, ServerExperience>
    {
        public IList<ServerExperience> GetAllExpForServer(ulong serverId);
    }
}