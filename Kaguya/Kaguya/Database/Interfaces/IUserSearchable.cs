namespace Kaguya.Database.Interfaces
{
    /// <summary>
    /// A data type inherits <see cref="IUserSearchable"/> if the object can be looked up
    /// by a UserId value. Any data type that is <see cref="IUserSearchable"/> guarantees
    /// that a UserId is present when searching for the object.
    /// </summary>
    public interface IUserSearchable
    {
        public ulong UserId { get; }
    }
}