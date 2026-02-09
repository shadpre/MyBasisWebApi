using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBasisWebApi.DataAccess.Configurations
{
    /// <summary>
    /// Entity Framework Core configuration for seeding default user roles.
    /// </summary>
    /// <remarks>
    /// Design decision: Seed predefined roles during migration to ensure they exist for authorization.
    /// Uses fixed GUIDs to maintain consistency across environments and migrations.
    /// 
    /// Roles defined:
    /// - Administrator: Full system access for management and configuration
    /// - User: Standard user access for regular application features
    /// 
    /// These roles are created when migrations are applied and persist in the database.
    /// New users are automatically assigned "User" role during registration.
    /// </remarks>
    public sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        /// <summary>
        /// Configures the IdentityRole entity with seed data.
        /// </summary>
        /// <param name="builder">The entity type builder for IdentityRole.</param>
        /// <remarks>
        /// Seed data ensures roles exist before any users are created.
        /// Fixed GUIDs prevent duplicate role creation during multiple migrations.
        /// NormalizedName is used by Identity for case-insensitive role lookups.
        /// </remarks>
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            // Seed default roles with fixed GUIDs for consistency
            builder.HasData(
                new IdentityRole
                {
                    // Administrator role - full system access
                    Id = "8065bf51-d0b6-47ce-b3eb-747de7a145c6",
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR" // Used for case-insensitive role checks
                },
                new IdentityRole
                {
                    // User role - standard application access (default for new users)
                    Id = "d307b037-a3e8-48a7-b284-a3fbcb0525b0",
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
        }
    }
}