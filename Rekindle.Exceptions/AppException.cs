using System.Net;

namespace Rekindle.Exceptions;

/// <summary>
/// Base exception class for all application-specific exceptions.
/// </summary>
public abstract class AppException : Exception
{
    /// <summary>
    /// Gets the HTTP status code that should be returned when this exception is thrown.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the error code that identifies the type of error.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets additional information about the error.
    /// </summary>
    public IDictionary<string, object>? Extensions { get; protected init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="errorCode">The error code that identifies the type of error.</param>
    /// <param name="innerException">The inner exception.</param>
    protected AppException(string message, HttpStatusCode statusCode, string errorCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}