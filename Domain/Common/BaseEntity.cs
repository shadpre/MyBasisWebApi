namespace MyBasisWebApi.Logic.Entities.Common
{
    /// <summary>
    /// Base class for all domain entities providing common auditing properties.
    /// </summary>
    /// <remarks>
    /// Design decision: Abstract base class enforces consistent auditing across all entities.
    /// All entities inherit tracking of creation and modification metadata.
    /// Uses integer primary key for high-volume entities. Consider GUID for aggregate roots
    /// in distributed scenarios (see copilot-instructions.md for guidance).
    /// 
    /// Properties use private setters to enforce immutability and encapsulation.
    /// Only domain methods should modify entity state.
    /// </remarks>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        /// <remarks>
        /// Integer primary key chosen for performance in high-volume scenarios.
        /// Database generates this value on INSERT.
        /// </remarks>
        public int Id { get; private set; }
        
        /// <summary>
        /// Gets the UTC timestamp when the entity was created.
        /// </summary>
        /// <remarks>
        /// Always stored in UTC to avoid timezone issues.
        /// Set automatically by domain factory methods or repository on creation.
        /// Immutable after creation for audit trail integrity.
        /// </remarks>
        public DateTime CreatedDate { get; private set; }
        
        /// <summary>
        /// Gets the username or identifier of who created the entity.
        /// </summary>
        /// <remarks>
        /// Nullable to support system-generated entities.
        /// Typically populated from authentication context (ClaimsPrincipal).
        /// </remarks>
        public string? CreatedBy { get; private set; }
        
        /// <summary>
        /// Gets the UTC timestamp when the entity was last modified.
        /// </summary>
        /// <remarks>
        /// Null for entities that have never been modified.
        /// Updated automatically by domain methods or repository on update.
        /// </remarks>
        public DateTime? LastModifiedDate { get; private set; }
        
        /// <summary>
        /// Gets the username or identifier of who last modified the entity.
        /// </summary>
        /// <remarks>
        /// Null for entities that have never been modified.
        /// Tracks last modifier for audit and compliance purposes.
        /// </remarks>
        public string? LastModifiedBy { get; private set; }
        
        /// <summary>
        /// Sets audit information for entity creation.
        /// </summary>
        /// <param name="createdBy">The username or identifier of the creator.</param>
        /// <remarks>
        /// Should be called by factory methods or repository layer when creating entities.
        /// Sets CreatedDate to current UTC time automatically.
        /// </remarks>
        protected void SetCreatedAudit(string? createdBy)
        {
            CreatedDate = DateTime.UtcNow;
            CreatedBy = createdBy;
        }
        
        /// <summary>
        /// Sets audit information for entity modification.
        /// </summary>
        /// <param name="modifiedBy">The username or identifier of the modifier.</param>
        /// <remarks>
        /// Should be called by domain methods or repository layer when updating entities.
        /// Sets LastModifiedDate to current UTC time automatically.
        /// </remarks>
        protected void SetModifiedAudit(string? modifiedBy)
        {
            LastModifiedDate = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
    }
}