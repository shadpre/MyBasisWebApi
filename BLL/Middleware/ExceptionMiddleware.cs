using FluentValidation;
using MyBasisWebApi.Logic.Exceptions;
using Microsoft.AspNetCore.Http; 
using Microsoft.Extensions.Logging; 
using System.Net; 
using System.Text.Json;

namespace MyBasisWebApi.Logic.Middleware
{
    /// <summary>
    /// Global exception handling middleware for the application.
    /// </summary>
    /// <remarks>
    /// Design decision: Centralized exception handling in middleware layer.
    /// Converts exceptions to appropriate HTTP status codes and error responses.
    /// Must be registered first in middleware pipeline to catch all exceptions.
    /// 
    /// Exception handling strategy:
    /// - Domain exceptions (NotFoundException, BadRequestException) → User-friendly errors
    /// - ValidationException (FluentValidation) → 400 with validation details
    /// - Unexpected exceptions → 500 with generic message (no internal details exposed)
    /// 
    /// All exceptions are logged for troubleshooting and monitoring.
    /// </remarks>
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            // Fail fast - validate dependencies at construction time
            ArgumentNullException.ThrowIfNull(next);
            ArgumentNullException.ThrowIfNull(logger);

            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to handle the HTTP request with exception handling.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Wraps the entire request pipeline in try-catch to capture all exceptions.
        /// Exceptions are logged and converted to appropriate HTTP responses.
        /// </remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Continue to next middleware - normal request flow
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log exception with structured logging for troubleshooting
                _logger.LogError(ex, 
                    "Unhandled exception occurred while processing {Method} {Path}", 
                    context.Request.Method,
                    context.Request.Path);
                
                // Convert exception to HTTP response
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions and writes appropriate HTTP responses.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        /// <remarks>
        /// Exception mapping:
        /// - NotFoundException → 404 Not Found
        /// - BadRequestException → 400 Bad Request
        /// - ValidationException → 400 Bad Request with validation errors
        /// - All others → 500 Internal Server Error with generic message
        /// 
        /// Security: Internal error details are never exposed to clients.
        /// </remarks>
        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            HttpStatusCode statusCode;
            ErrorDetails errorDetails;

            switch (ex)
            {
                case NotFoundException notFoundException:
                    // Resource not found - expected domain exception
                    statusCode = HttpStatusCode.NotFound;
                    errorDetails = new ErrorDetails
                    {
                        ErrorType = "Not Found",
                        ErrorMessage = notFoundException.Message
                    };
                    break;

                case BadRequestException badRequestException:
                    // Invalid request - expected domain exception
                    statusCode = HttpStatusCode.BadRequest;
                    errorDetails = new ErrorDetails
                    {
                        ErrorType = "Bad Request",
                        ErrorMessage = badRequestException.Message
                    };
                    break;

                case ValidationException validationException:
                    // FluentValidation failure - return all validation errors
                    statusCode = HttpStatusCode.BadRequest;
                    errorDetails = new ErrorDetails
                    {
                        ErrorType = "Validation Failure",
                        ErrorMessage = "One or more validation errors occurred.",
                        // Include detailed validation errors for client-side display
                        ValidationErrors = validationException.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray())
                    };
                    break;

                default:
                    // Unexpected exception - log full details but return generic message
                    statusCode = HttpStatusCode.InternalServerError;
                    errorDetails = new ErrorDetails
                    {
                        ErrorType = "Internal Server Error",
                        // Security: Don't expose internal details in production
                        ErrorMessage = "An error occurred while processing your request."
                    };
                    break;
            }

            // Serialize error response using System.Text.Json (modern .NET standard)
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false // Compact JSON for network efficiency
            };
            
            string response = JsonSerializer.Serialize(errorDetails, options);
            context.Response.StatusCode = (int)statusCode;
            
            return context.Response.WriteAsync(response);
        }
    }

    /// <summary>
    /// Represents the standardized error response structure.
    /// </summary>
    /// <remarks>
    /// Design decision: Consistent error format across all API endpoints.
    /// Clients can rely on this structure for error handling.
    /// ValidationErrors property is only populated for validation failures.
    /// </remarks>
    public sealed class ErrorDetails
    {
        /// <summary>
        /// Gets or sets the type of error that occurred.
        /// </summary>
        /// <remarks>
        /// Examples: "Not Found", "Bad Request", "Validation Failure", "Internal Server Error"
        /// </remarks>
        public string ErrorType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable error message.
        /// </summary>
        /// <remarks>
        /// Safe to display to end users - never contains internal details.
        /// </remarks>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets validation errors grouped by property name.
        /// </summary>
        /// <remarks>
        /// Only populated for ValidationException (FluentValidation failures).
        /// Key: Property name that failed validation
        /// Value: Array of error messages for that property
        /// Example: { "Email": ["Email is required", "Email must be valid format"] }
        /// </remarks>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}