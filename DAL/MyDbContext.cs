using DAL.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DAL
{
    /// <summary>
    /// Database context for the application.
    /// Inherits from IdentityDbContext with ApiUser.
    /// </summary>
    public class MyDbContext : IdentityDbContext<ApiUser>
    {
        /// <summary>
        /// Initializes a new instance of the MyDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public MyDbContext(DbContextOptions options) : base(options)
        {

        }

        /// <summary>
        /// Configures the model for the DbContext.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
        }

        /// <summary>
        /// Factory for creating instances of MyDbContext during design-time.
        /// </summary>
        public class HotelListingDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
        {
            /// <summary>
            /// Creates a new instance of MyDbContext.
            /// </summary>
            /// <param name="args">Arguments passed by the design-time tools.</param>
            /// <returns>A new instance of MyDbContext.</returns>
            public MyDbContext CreateDbContext(string[] args)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
                var conn = config.GetConnectionString("MyDbConnectionString");
                optionsBuilder.UseSqlServer(conn);
                return new MyDbContext(optionsBuilder.Options);
            }
        }
    }
}