namespace Domain.Common
{
    /// <summary>
    /// Base class for all entities
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// The unique identifier for the entity
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// When the entity was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Who created the entity
        /// </summary>
        public string? CreatedBy { get; set; }
        
        /// <summary>
        /// When the entity was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// Who last modified the entity
        /// </summary>
        public string? LastModifiedBy { get; set; }
    }
}