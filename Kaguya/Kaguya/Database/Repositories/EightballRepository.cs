using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;

namespace Kaguya.Database.Repositories
{
    public class EightballRepository : RepositoryBase<Eightball>, IEightBallRepository
    {
        public EightballRepository(KaguyaDbContext dbContext) : base(dbContext) { }
    }
}