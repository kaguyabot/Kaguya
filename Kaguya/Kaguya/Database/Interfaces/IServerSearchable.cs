namespace Kaguya.Database.Interfaces
{
    /// <summary>
    /// A data type inherits <see cref="IServerSearchable"/> if the object can be looked up
    /// by a ServerId value. Any data type that is <see cref="IServerSearchable"/> guarantees
    /// that a ServerId is present when searching for the object.
    /// </summary>
    public interface IServerSearchable
    {
        public ulong ServerId { get; }
    }
}