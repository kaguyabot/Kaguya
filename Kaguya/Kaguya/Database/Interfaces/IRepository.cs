using System.Threading.Tasks;

namespace Kaguya.Database.Interfaces
{
    public interface IRepository<in T, TValue> where TValue : class
    {
        public Task<TValue> GetAsync(T key);
        public Task DeleteAsync(T key);
        public Task UpdateAsync(TValue value);
        public Task InsertAsync(TValue value);
    }
    
    public interface IRepository<in T1, in T2, TValue> where TValue : class
    {
        public Task<TValue> GetAsync(T1 key1, T2 key2);
        public Task DeleteAsync(T1 key1, T2 key2);
        public Task UpdateAsync(TValue value);
        public Task InsertAsync(TValue value);
    }
    
    public interface IRepository<in T1, in T2, in T3, TValue> where TValue : class
    {
        public Task<TValue> GetAsync(T1 key1, T2 key2, T3 key3);
        public Task DeleteAsync(T1 key1, T2 key2, T3 key3);
        public Task UpdateAsync(TValue value);
        public Task InsertAsync(TValue value);
    }
}