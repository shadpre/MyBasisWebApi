using AspNetCoreRateLimit;
using FluentValidation;
using MyBasisWebApi.Logic;
using MyBasisWebApi.Logic.Configuration;
using MyBasisWebApi.Logic.Handlers.Behaviors;
using MyBasisWebApi.Logic.Interfaces;
using MyBasisWebApi.Logic.Middleware;
using MyBasisWebApi.Logic.Services.Authentication;
using MyBasisWebApi.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.OData; 
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using Serilog;
using System.Reflection;
using System.Text;

namespace MyBasisWebApi.Presentation
{
    /// <summary>
    /// Main program class for Web API application.
    /// </summary>
    /// <remarks>
    /// Configures all application services, middleware pipeline, and hosting.
    /// Follows ScanitechDanmark architectural standards for Web API applications.
    /// </remarks>
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command-line arguments for configuration override.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==================== Database Configuration ====================
            /// <summary>
            /// Configures Entity Framework Core with SQL Server.
            /// </summary>
            /// <remarks>
            /// Design decision: Using EF Core because we own and control the database schema.
            /// Connection string is stored in appsettings.json and should be overridden
            /// with environment-specific values in production.
            /// </remarks>
            var connectionString = builder.Configuration.GetConnectionString("MyDbConnectionString");
            builder.Services.AddDbContext<MyDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // ==================== Identity Configuration ====================
            /// <summary>
            /// Configures ASP.NET Core Identity for user management.
            /// </summary>
            /// <remarks>
            /// Design decision: Using Identity Core with custom ApiUser entity.
            /// Identity provides built-in password hashing, validation, and user management.
            /// Token provider issuer must match JWT issuer for refresh token generation.
            /// </remarks>
            var provider = builder.Configuration.GetSection("JwtSettings:Issuer").Value;

            builder.Services.AddIdentityCore<ApiUser>()
                .AddRoles<IdentityRole>()
                .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>(provider)
                .AddEntityFrameworkStores<MyDbContext>()
                .AddDefaultTokenProviders();

            // ==================== Swagger/OpenAPI Configuration ====================
            /// <summary>
            /// Configures Swagger for API documentation and testing.
            /// </summary>
            /// <remarks>
            /// Swagger provides interactive API documentation and testing UI.
            /// JWT Bearer authentication is configured for "Try it out" functionality.
            /// Users must obtain token from /api/account/login first, then use it in Swagger UI.
            /// </remarks>
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My Basis Web API", Version = "v1" });
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
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
                            Scheme = "oauth2",
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

            // ==================== Logging Configuration ====================
            /// <summary>
            /// Configures structured logging with Serilog.
            /// </summary>
            /// <remarks>
            /// Design decision: Use Serilog for structured logging with file and console sinks.
            /// Reads configuration from appsettings.json for environment-specific log levels.
            /// Daily rolling file prevents unbounded log growth.
            /// </remarks>
            builder.Host.UseSerilog((ctx, lc) => 
                lc.WriteTo.Console()
                  .ReadFrom.Configuration(ctx.Configuration));

            // ==================== Mapping Configuration ====================
            /// <summary>
            /// Configures AutoMapper for object-to-object mapping.
            /// </summary>
            /// <remarks>
            /// AutoMapper handles DTO to entity conversions, preventing manual mapping code.
            /// Mapping profiles are defined in MapperConfig class.
            /// </remarks>
            builder.Services.AddAutoMapper(typeof(MapperConfig));

            // ==================== MediatR Configuration ====================
            /// <summary>
            /// Configures MediatR for CQRS pattern implementation.
            /// </summary>
            /// <remarks>
            /// Design decision: MediatR separates commands (write) from queries (read) for CQRS.
            /// Handlers are automatically discovered from Logic assembly.
            /// Pipeline behaviors (validation, logging, transactions) are applied to all requests.
            /// </remarks>
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(MapperConfig).Assembly);
                
                // Add validation behavior to pipeline - validates before handler execution
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // ==================== FluentValidation Configuration ====================
            /// <summary>
            /// Configures FluentValidation for automatic request validation.
            /// </summary>
            /// <remarks>
            /// Validators are automatically discovered from Logic assembly.
            /// Validation runs in MediatR pipeline before handlers execute.
            /// Validation failures throw ValidationException, converted to 400 Bad Request.
            /// </remarks>
            builder.Services.AddValidatorsFromAssembly(typeof(MapperConfig).Assembly);

            // ==================== Application Services ====================
            /// <summary>
            /// Registers application services for dependency injection.
            /// </summary>
            /// <remarks>
            /// REFACTORED: Removed generic repository anti-pattern (violates coding standards).
            /// Services should use DbContext directly or aggregate-specific repositories.
            /// </remarks>
            builder.Services.AddScoped<IAuthManager, AuthManager>();

            // ==================== JWT Authentication Configuration ====================
            /// <summary>
            /// Configures JWT Bearer authentication.
            /// </summary>
            /// <remarks>
            /// Security: Validates issuer, audience, lifetime, and signing key.
            /// All parameters must match between token generation and validation.
            /// IssuerSigningKey uses symmetric encryption - keep secret key secure.
            /// Key should be stored in user secrets or Azure Key Vault, not appsettings.json.
            /// </remarks>
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = jwtSettings["Key"];
            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

            // ==================== Response Caching Configuration ====================
            /// <summary>
            /// Configures response caching for improved performance.
            /// </summary>
            /// <remarks>
            /// Caches GET responses to reduce database load and improve response times.
            /// MaximumBodySize limits cached response size to 1KB.
            /// Case-sensitive paths ensure different URLs don't share cache.
            /// </remarks>
            builder.Services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024;
                options.UseCaseSensitivePaths = true;
            });

            // ==================== Rate Limiting Configuration ====================
            /// <summary>
            /// Configures IP-based rate limiting to prevent API abuse.
            /// </summary>
            /// <remarks>
            /// Rate limiting protects API from excessive requests and potential DDoS attacks.
            /// Configuration is loaded from appsettings.json IpRateLimitOptions section.
            /// Limits are enforced per IP address with in-memory storage.
            /// For production with multiple instances, consider distributed cache (Redis).
            /// </remarks>
            builder.Services.AddOptions();
            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimitOptions"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // ==================== Controllers & OData Configuration ====================
            /// <summary>
            /// Configures MVC controllers with OData query support.
            /// </summary>
            /// <remarks>
            /// OData enables advanced query capabilities through URL parameters:
            /// - $select: Choose which fields to return
            /// - $filter: Filter results with expressions
            /// - $orderby: Sort results
            /// Example: /api/items?$select=name&$filter=price gt 100&$orderby=name
            /// </remarks>
            builder.Services.AddControllers().AddOData(options =>
            {
                options.Select().Filter().OrderBy();
            });

            var app = builder.Build();

            // ==================== Middleware Pipeline Configuration ====================
            /// <summary>
            /// Configures the HTTP request pipeline.
            /// </summary>
            /// <remarks>
            /// Middleware order is critical for correct application behavior:
            /// 1. Exception handling (catches all errors)
            /// 2. HTTPS redirection (enforces secure connections)
            /// 3. Logging (tracks requests)
            /// 4. CORS (handles cross-origin requests)
            /// 5. Authentication (identifies user)
            /// 6. Authorization (enforces permissions)
            /// 7. Rate limiting (prevents abuse)
            /// 8. Controllers (handles business logic)
            /// </remarks>

            // Development tools - safe to expose only in development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Global exception handler - must be first to catch all errors
            app.UseMiddleware<ExceptionMiddleware>();

            // HTTPS redirection - enforces secure connections
            app.UseHttpsRedirection();

            // Request logging - tracks all HTTP requests for diagnostics
            app.UseSerilogRequestLogging();

            // CORS - must be before Authentication/Authorization
            app.UseCors("AllowAll");

            // Response caching - improves performance for GET requests
            app.UseResponseCaching();

            /// <summary>
            /// Custom middleware to set cache control headers.
            /// </summary>
            /// <remarks>
            /// Sets cache headers for all responses:
            /// - Public: Allows caching by proxies and CDNs
            /// - MaxAge: Caches for 10 seconds
            /// - Vary: Ensures different encodings get separate cache entries
            /// </remarks>
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(10)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };

                await next();
            });

            // Authentication - identifies user from JWT token
            app.UseAuthentication();
            
            // Authorization - enforces role/policy requirements
            app.UseAuthorization();

            // Rate limiting - prevents API abuse (should be after auth to identify users)
            app.UseIpRateLimiting();

            // Controller routing - handles business logic
            app.MapControllers();

            // Start the application and block until shutdown
            app.Run();
        }
    }
}