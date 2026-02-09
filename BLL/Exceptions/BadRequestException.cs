namespace MyBasisWebApi.Logic.Exceptions
{
    /// <summary>
    /// Exception thrown when a request violates business rules or contains invalid data.
    /// </summary>
    /// <remarks>
    /// Design decision: Domain exception for business rule violations and invalid requests.
    /// Caught by ExceptionMiddleware and converted to 400 Bad Request HTTP response.
    /// 
    /// Use cases:
    /// - Business rule validation failures (e.g., "Cannot cancel completed order")
    /// - Invalid state transitions (e.g., "Order must be in Draft status to add items")
    /// - Data consistency violations (e.g., "Duplicate order number")
    /// 
    /// Note: For input validation failures, use FluentValidation instead.
    /// BadRequestException is for business logic violations, not input format errors.
    /// </remarks>
    public sealed class BadRequestException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="message">The user-friendly error message describing the business rule violation.</param>
        /// <remarks>
        /// Error message should be clear and actionable, explaining what went wrong and how to fix it.
        /// Example: "Cannot delete order because it has already been shipped. Only draft orders can be deleted."
        /// Message is exposed to API consumers, so avoid internal implementation details.
        /// </remarks>
        public BadRequestException(string message) : base(message)
        {
            // Simple pass-through to base - no additional state required
            // Message should be descriptive enough for client-side error handling
        }
    }
}