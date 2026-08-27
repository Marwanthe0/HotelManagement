using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // A unique index violation means a concurrent request already inserted the same
        // value (duplicate RoomNumber / Email). Report it as a conflict rather than a 500.
        var isDuplicateKey = exception is DbUpdateException dbUpdateException
            && IsUniqueConstraintViolation(dbUpdateException);

        var statusCode = exception switch
        {
            _ when isDuplicateKey => StatusCodes.Status409Conflict,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "An unhandled server exception occurred.");
        }
        else
        {
            _logger.LogWarning("Request resulted in HTTP {StatusCode}: {Message}", statusCode, exception.Message);
        }

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal Server Error"
        };

        // Never leak internal details (stack traces, SQL, provider messages) to clients for 500 errors.
        var detail = statusCode switch
        {
            StatusCodes.Status409Conflict =>
                "The value you supplied conflicts with an existing record.",
            StatusCodes.Status500InternalServerError =>
                "An unexpected error occurred while processing your request.",
            _ => exception.Message
        };

        var response = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }

    // SQL Server reports unique index/constraint violations as error 2601/2627.
    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message;

        return message is not null
            && (message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || message.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase)
                || message.Contains("2601")
                || message.Contains("2627"));
    }
}
