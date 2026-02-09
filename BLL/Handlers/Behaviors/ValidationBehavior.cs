using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MyBasisWebApi.Logic.Handlers.Behaviors;

/// <summary>
/// MediatR pipeline behavior that performs FluentValidation before handler execution.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
/// <remarks>
/// Design decision: Use pipeline behavior to centralize validation logic.
/// All commands/queries are automatically validated before reaching handlers.
/// 
/// Validation failures throw ValidationException which is caught by ExceptionMiddleware
/// and converted to 400 Bad Request with detailed error messages.
/// 
/// If no validators exist for a request type, the behavior passes through to the handler.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">Collection of validators for the request type.</param>
    /// <param name="logger">The logger instance.</param>
    /// <remarks>
    /// Validators are automatically discovered and injected by FluentValidation.
    /// If no validators exist for TRequest, the collection will be empty.
    /// </remarks>
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <summary>
    /// Handles the validation of the request before passing to the next handler.
    /// </summary>
    /// <param name="request">The request being validated.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    /// <remarks>
    /// Validation flow:
    /// 1. Check if any validators exist for this request type
    /// 2. If validators exist, run all validators
    /// 3. If any validation failures occur, throw ValidationException
    /// 4. If validation passes, continue to handler
    /// 
    /// All validators are run to provide complete error feedback.
    /// Fail fast - validation errors prevent handler execution.
    /// </remarks>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip validation if no validators registered for this request type
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context for the request
        var context = new ValidationContext<TRequest>(request);

        // Run all validators and collect results
        // Parallel validation improves performance for multiple validators
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Aggregate all validation failures from all validators
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If any validation failures exist, throw exception
        if (failures.Count != 0)
        {
            // Log validation failure for diagnostics
            _logger.LogWarning(
                "Validation failed for {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

            // Throw ValidationException - ExceptionMiddleware will convert to 400 Bad Request
            throw new ValidationException(failures);
        }

        // Validation passed - continue to handler
        return await next();
    }
}
