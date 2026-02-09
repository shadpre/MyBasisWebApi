using AutoMapper;
using MyBasisWebApi.Logic.Models.Users;
using MyBasisWebApi.DataAccess;

namespace MyBasisWebApi.Logic
{
    /// <summary>
    /// AutoMapper profile configuration for DTO to entity mappings.
    /// </summary>
    /// <remarks>
    /// Design decision: Centralized mapping configuration using AutoMapper profiles.
    /// Profiles are automatically discovered by AutoMapper during service registration.
    /// 
    /// Benefits:
    /// - Eliminates manual property-by-property mapping code
    /// - Provides compile-time safety for property name changes
    /// - Supports convention-based mapping (matching property names)
    /// - Enables custom mapping logic when needed
    /// 
    /// All mapping profiles should be added to this class or create separate Profile classes
    /// in a Mapping folder for better organization in larger projects.
    /// </remarks>
    public sealed class MapperConfig : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapperConfig"/> class and defines all mappings.
        /// </summary>
        /// <remarks>
        /// Mappings defined:
        /// - ApiUserDto ↔ ApiUser (bidirectional for registration and profile operations)
        /// 
        /// ReverseMap() creates bidirectional mapping with same configuration.
        /// Convention-based mapping matches properties by name (FirstName → FirstName, etc.).
        /// Password from DTO is not mapped to entity (handled separately by Identity).
        /// </remarks>
        public MapperConfig()
        {
            // Map registration DTO to entity and vice versa
            // Properties with matching names are mapped automatically (FirstName, LastName, Email)
            // Password is intentionally not mapped - handled by Identity's password hasher
            CreateMap<ApiUserDto, ApiUser>()
                .ReverseMap(); // Allows mapping in both directions
        }
    }
}