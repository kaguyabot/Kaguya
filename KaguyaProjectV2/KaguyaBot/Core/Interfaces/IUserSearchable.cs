namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    public interface IUserSearchable<T> where T : class, IKaguyaQueryable<T>
    {
        ulong UserId { get; }
    }
}