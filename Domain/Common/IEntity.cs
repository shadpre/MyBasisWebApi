namespace Domain.Common
{
    /// <summary>
    /// Base interface for all entities
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// The unique identifier for the entity
        /// </summary>
        int Id { get; set; }
    }
}