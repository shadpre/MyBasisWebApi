using AspNetCoreRateLimit;
using MyBasisWebApi.Logic;
using MyBasisWebApi.Logic.Interfaces;
using MyBasisWebApi.Logic.Middleware;
using MyBasisWebApi.Logic.Services.Authentication;
using MyBasisWebApi.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.OData; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.OpenApi.Models; 
using Serilog; 

namespace MyBasisWebApi.Presentation
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method that starts the application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args); // Create a new WebApplication builder

            var connectionString = builder.Configuration.GetConnectionString("MyDbConnectionString"); // Get the connection string from configuration
            builder.Services.AddDbContext<MyDbContext>(options =>
            {
                options.UseSqlServer(connectionString); // Configure DbContext to use SQL Server with the connection string
            });

            var provider = builder.Configuration.GetSection("JwtSettings:Issuer").Value; // Get the JWT issuer from configuration

            builder.Services.AddIdentityCore<ApiUser>() // Add ASP.NET Core Identity services with ApiUser as the user type
                .AddRoles<IdentityRole>() // Add role support
                .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>(provider) // Add token provider with JWT issuer as provider
                .AddEntityFrameworkStores<MyDbContext>() // Use MyDbContext for storing identity data
                .AddDefaultTokenProviders(); // Add default token providers

            builder.Services.AddEndpointsApiExplorer(); // Add API explorer for endpoint documentation
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Listing API", Version = "v1" }); // Configure Swagger document with title and version
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'", // Description of JWT authorization header
                    Name = "Authorization", // Name of the header parameter
                    In = ParameterLocation.Header, // Location of the parameter (header)
                    Type = SecuritySchemeType.ApiKey, // Type of security scheme (API key)
                    Scheme = JwtBearerDefaults.AuthenticationScheme // Scheme name (JWT Bearer)
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            },
                            Scheme = "0auth2",
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            // ==================== CORS Configuration ====================
            /// <summary>
            /// Configures CORS (Cross-Origin Resource Sharing) policy.
            /// </summary>
            /// <remarks>
            /// SECURITY FIX: Uses explicit allowed origins from configuration instead of AllowAnyOrigin.
            /// AllowAnyOrigin is a security vulnerability that allows malicious websites to call the API.
            /// 
            /// Allowed origins should be configured in appsettings.json:
            /// {
            ///   "AllowedOrigins": ["https://localhost:5001", "https://yourdomain.com"]
            /// }
            /// 
            /// AllowCredentials is required for sending cookies and authentication tokens.
            /// Update AllowedOrigins in appsettings.json for each environment.
            /// </remarks>
            var allowedOrigins = builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>() 
                ?? new[] { "https://localhost:5001" }; // Safe default for development

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.WithOrigins(allowedOrigins)  // ? Explicit origins only (SECURITY FIX)
                          .AllowAnyMethod()              // Allow GET, POST, PUT, DELETE, etc.
                          .AllowAnyHeader()              // Allow custom headers (Authorization, etc.)
                          .AllowCredentials());          // Allow cookies and auth tokens
            });

            builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration)); // Configure Serilog for logging

            builder.Services.AddAutoMapper(typeof(MapperConfig)); // Add AutoMapper services with MapperConfig

            // ==================== Application Services ====================
            /// <summary>
            /// Registers application services for dependency injection.
            /// </summary>
            /// <remarks>
            /// REFACTORED: Removed generic repository anti-pattern (violates coding standards).
            /// Services should use DbContext directly or aggregate-specific repositories.
            /// </remarks>
            builder.Services.AddScoped<IAuthManager, AuthManager>(); // Add scoped service for authentication manager

            builder.Services.AddAuthentication().AddJwtBearer(); // Add authentication services with JWT Bearer scheme

            builder.Services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024; // Set maximum body size for response caching
                options.UseCaseSensitivePaths = true; // Enable case-sensitive paths for response caching
            });

            // Add IP rate limiting services
            builder.Services.AddOptions(); // Add options services
            builder.Services.AddMemoryCache(); // Add memory cache services
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimitOptions")); // Configure IP rate limit options from configuration
            builder.Services.AddInMemoryRateLimiting(); // Add in-memory rate limiting services
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>(); // Add singleton service for rate limit configuration

            builder.Services.AddControllers().AddOData(options =>
            {
                options.Select().Filter().OrderBy(); // Enable OData support with select, filter, and order by options
            });

            var app = builder.Build(); // Build the application

            if (app.Environment.IsDevelopment())
            {
                // Development-specific configuration can be added here if needed
            }

            app.UseSwagger(); // Enable Swagger middleware for generating Swagger JSON endpoint and UI
            app.UseSwaggerUI(); // Enable Swagger UI middleware

            app.UseSerilogRequestLogging(); // Enable Serilog request logging middleware

            app.UseMiddleware<ExceptionMiddleware>(); // Use custom exception handling middleware

            app.UseHttpsRedirection(); // Enable HTTPS redirection middleware

            app.UseCors("AllowAll"); // Enable CORS middleware with "AllowAll" policy

            app.UseResponseCaching(); // Enable response caching middleware

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(10) // Set cache control headers with max age of 10 seconds
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" }; // Set vary header to "Accept-Encoding"

                await next(); // Call the next middleware in the pipeline
            });

            app.UseAuthentication(); // Enable authentication middleware
            app.UseAuthorization(); // Enable authorization middleware

            app.UseIpRateLimiting(); // Use IP rate limiting middleware

            app.MapControllers(); // Map controller routes

            app.Run(); // Run the application
        }
    }
}