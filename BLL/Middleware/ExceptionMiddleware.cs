using MyBasisWebApi.Logic.Exceptions;
using Microsoft.AspNetCore.Http; 
using Microsoft.Extensions.Logging; 
using Newtonsoft.Json; 
using System.Net; 

namespace MyBasisWebApi.Logic.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions in the application.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next; // Delegate for the next middleware in the pipeline
        private readonly ILogger<ExceptionMiddleware> _logger; // Logger instance for logging

        /// <summary>
        /// Initializes a new instance of the ExceptionMiddleware class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next; // Assign the next middleware delegate
            _logger = logger; // Assign the logger instance
        }

        /// <summary>
        /// Invokes the middleware to handle the HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Call the next middleware in the pipeline
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong while processing {context.Request.Path}"); // Log the exception with error message and request path
                await HandleExceptionAsync(context, ex); // Handle the exception and write the response
            }
        }

        /// <summary>
        /// Handles the exception and writes the response.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json"; // Set response content type to JSON
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError; // Default status code is 500 Internal Server Error
            var errorDetails = new ErrorDetails
            {
                ErrorType = "Failure", // Default error type is "Failure"
                ErrorMessage = ex.Message, // Set error message to exception message
            };

            switch (ex)
            {
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound; // Set status code to 404 Not Found for NotFoundException
                    errorDetails.ErrorType = "Not Found"; // Set error type to "Not Found"
                    break;
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest; // Set status code to 400 Bad Request for BadRequestException
                    errorDetails.ErrorType = "Bad Request"; // Set error type to "Bad Request"
                    break;
                default:
                    break;
            }

            string response = JsonConvert.SerializeObject(errorDetails); // Serialize error details to JSON string
            context.Response.StatusCode = (int)statusCode; // Set response status code
            return context.Response.WriteAsync(response); // Write JSON response to HTTP context
        }
    }

    /// <summary>
    /// Represents the details of an error.
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// Gets or sets the type of the error.
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}