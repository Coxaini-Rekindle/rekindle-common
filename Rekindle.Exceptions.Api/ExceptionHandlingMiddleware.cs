using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Rekindle.Exceptions.Api;

internal class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, type, modelState, extensions) = ExtractExceptionInfo(exception);
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = CreateProblemDetails(context, statusCode, title, detail, type, modelState, extensions);
        await WriteProblemDetailsResponseAsync(context, problemDetails);
    }

    private (HttpStatusCode statusCode, string title, string? detail, string type, ModelStateDictionary? modelState,
        IDictionary<string, object>? extensions) ExtractExceptionInfo(Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        string? detail = null;
        var title = "An unexpected error occurred.";
        var type = "internal_server_error";
        ModelStateDictionary? modelState = null;
        IDictionary<string, object>? extensions = null;

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            title = appException.Message;
            type = appException.ErrorCode;
            if (appException.Extensions != null)
            {
                extensions = appException.Extensions;
            }

            if (appException is ValidationException validationException)
            {
                modelState = CreateModelState(validationException);
            }
        }
        else
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing the request");

            if (_environment.IsDevelopment())
            {
                title = exception.Message;
                detail = exception.StackTrace;
            }
        }

        return (statusCode, title, detail, type, modelState, extensions);
    }

    private static ModelStateDictionary CreateModelState(ValidationException validationException)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in validationException.Errors)
        {
            foreach (var message in error.Value)
            {
                modelState.AddModelError(error.Key, message);
            }
        }

        return modelState;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string? detail,
        string type,
        ModelStateDictionary? modelState,
        IDictionary<string, object>? extensions)
    {
        ProblemDetails problemDetails;

        if (modelState != null)
        {
            var validationProblemDetails = new ValidationProblemDetails(modelState)
            {
                Status = (int)statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.TraceIdentifier
                }
            };


            problemDetails = validationProblemDetails;
        }
        else
        {
            problemDetails = new ProblemDetails()
            {
                Status = (int)statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.TraceIdentifier
                }
            };

            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions.Add(extension.Key, extension.Value);
                }
            }
        }

        return problemDetails;
    }

    private static async Task WriteProblemDetailsResponseAsync(HttpContext context, ProblemDetails problemDetails)
    {
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonSerializerOptions));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}