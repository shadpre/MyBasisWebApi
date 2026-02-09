namespace MyBasisWebApi.Logic.Entities.Common
{
    /// <summary>
    /// Base interface defining the contract for all domain entities.
    /// </summary>
    /// <remarks>
    /// Design decision: Minimal interface defining only the identity requirement.
    /// All entities must have a unique identifier for persistence and retrieval.
    /// 
    /// Use this interface when you need to work with entities generically,
    /// such as in repository base methods or common utilities.
    /// 
    /// Integer Id is the default. For aggregate roots in distributed systems,
    /// consider using Guid instead (create IEntity&lt;TKey&gt; variant).
    /// </remarks>
    public interface IEntity
    {
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        /// <remarks>
        /// Read-only at interface level to prevent external modification.
        /// Implementations should use private setter to enforce immutability.
        /// Database typically generates this value on first save.
        /// </remarks>
        int Id { get; }
    }
}