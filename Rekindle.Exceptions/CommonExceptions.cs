using System.Net;

namespace Rekindle.Exceptions;

/// <summary>
/// Exception thrown when a validation error occurs.
/// </summary>
public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.",
            HttpStatusCode.BadRequest,
            "validation_error")
    {
        Errors = errors;
    }
}