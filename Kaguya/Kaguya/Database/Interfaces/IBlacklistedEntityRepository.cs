using System.Threading.Tasks;
using Kaguya.Database.Model;

namespace Kaguya.Database.Interfaces
{
    public interface IBlacklistedEntityRepository : IRepository<ulong, BlacklistedEntity>
    {
    }
}