namespace KaguyaProjectV2.KaguyaBot.Core.Interfaces
{
    /// <summary>
    /// Represents a unique <see cref="IKaguyaQueryable{T}"/> object. Objects that implement
    /// this <see cref="IKaguyaUnique{T}"/> interface must have a [PrimaryKey] tag.
    /// </summary>
    public interface IKaguyaUnique<T> where T : class, IKaguyaQueryable<T>
    { }
}