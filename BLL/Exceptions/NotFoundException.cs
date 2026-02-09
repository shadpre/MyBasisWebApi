namespace MyBasisWebApi.Logic.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested entity is not found in the database.
    /// </summary>
    /// <remarks>
    /// Design decision: Domain exception for resource not found scenarios.
    /// Caught by ExceptionMiddleware and converted to 404 Not Found HTTP response.
    /// 
    /// Use cases:
    /// - Entity lookup by ID returns null
    /// - Required reference data is missing
    /// - User attempts to access non-existent resource
    /// 
    /// Example: throw new NotFoundException("Order", orderId);
    /// Results in: "Order with id (12345) was not found"
    /// </remarks>
    public sealed class NotFoundException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="name">The name of the entity type that was not found (e.g., "Order", "Customer").</param>
        /// <param name="key">The identifier value that was used in the lookup.</param>
        /// <remarks>
        /// Constructs a user-friendly error message in the format: "{name} with id ({key}) was not found".
        /// This message is safe to display to end users as it doesn't expose internal system details.
        /// </remarks>
        public NotFoundException(string name, object key) : base($"{name} with id ({key}) was not found")
        {
            // Message construction in base call keeps exception creation simple
            // No additional state needed - exception message contains all context
        }
    }
}