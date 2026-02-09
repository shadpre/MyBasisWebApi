using MyBasisWebApi.DataAccess.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MyBasisWebApi.DataAccess
{
    /// <summary>
    /// Application database context for user management and authentication.
    /// </summary>
    /// <remarks>
    /// Design decision: Inherits from IdentityDbContext to leverage ASP.NET Core Identity
    /// for user management, authentication, and role-based authorization.
    /// 
    /// Keeps DbContext thin - no business logic, only data access configuration.
    /// Entity configurations are separated into IEntityTypeConfiguration classes.
    /// 
    /// Uses scoped lifetime (configured in Program.cs) - new instance per HTTP request.
    /// </remarks>
    public sealed class MyDbContext : IdentityDbContext<ApiUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        /// <remarks>
        /// Constructor injection allows options to be configured at startup.
        /// Options include connection string and SQL Server provider configuration.
        /// </remarks>
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the entity model using Fluent API.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
        /// <remarks>
        /// Best practice: Use ApplyConfiguration to keep configurations organized.
        /// Each entity has its own configuration class implementing IEntityTypeConfiguration.
        /// Base call is required to configure Identity tables (Users, Roles, etc.).
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Identity tables (AspNetUsers, AspNetRoles, etc.)
            base.OnModelCreating(modelBuilder);
            
            // Apply custom configurations
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
        }

        /// <summary>
        /// Factory for creating instances of MyDbContext during design-time operations.
        /// </summary>
        /// <remarks>
        /// Design-time factory enables EF Core tools to create DbContext for:
        /// - Generating migrations: dotnet ef migrations add
        /// - Updating database: dotnet ef database update
        /// - Scaffolding: dotnet ef dbcontext scaffold
        /// 
        /// Reads connection string from appsettings.json in the project root.
        /// Only used by EF Core CLI tools, not at runtime.
        /// </remarks>
        public sealed class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
        {
            /// <summary>
            /// Creates a new instance of MyDbContext for design-time operations.
            /// </summary>
            /// <param name="args">Arguments passed by the design-time tools.</param>
            /// <returns>A configured MyDbContext instance.</returns>
            /// <remarks>
            /// Configuration is loaded from appsettings.json in the current directory.
            /// Connection string must match the one used at runtime.
            /// </remarks>
            public MyDbContext CreateDbContext(string[] args)
            {
                // Load configuration from appsettings.json
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Configure DbContext with SQL Server provider
                var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
                var conn = config.GetConnectionString("MyDbConnectionString");
                optionsBuilder.UseSqlServer(conn);
                
                return new MyDbContext(optionsBuilder.Options);
            }
        }
    }
}