namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    public interface IServerSearchable<T> where T : class, IKaguyaQueryable<T>
    {
        ulong ServerId { get; }
    }
}
