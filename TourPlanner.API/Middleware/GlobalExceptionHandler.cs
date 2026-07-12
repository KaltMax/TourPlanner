using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.BLL.Exceptions;
using TourPlanner.Logging;

namespace TourPlanner.API.Middleware
{
    /// <summary>
    /// Centralized exception handler that maps known domain/service exceptions to
    /// the appropriate HTTP status codes and writes an RFC 9457 ProblemDetails response
    /// via <see cref="IProblemDetailsService"/> (so the format matches framework-generated
    /// errors). Registered with AddExceptionHandler and invoked by UseExceptionHandler.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILoggerWrapper<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILoggerWrapper<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title) = MapException(exception);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, $"Unhandled exception on {httpContext.Request.Path}");
            }
            else
            {
                _logger.LogWarning(exception, $"Handled exception on {httpContext.Request.Path}");
            }

            httpContext.Response.StatusCode = statusCode;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails =
                {
                    Status = statusCode,
                    Title = title,
                    Instance = httpContext.Request.Path
                }
            });
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            GeocodingException              => (StatusCodes.Status400BadRequest, "Failed to geocode the provided locations."),
            RoutingException                => (StatusCodes.Status502BadGateway, "Unable to retrieve routing information."),
            TourNameAlreadyExistsException  => (StatusCodes.Status409Conflict, exception.Message),
            TourServiceNotFoundException    => (StatusCodes.Status404NotFound, exception.Message),
            TourLogServiceNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            TourServiceException            => (StatusCodes.Status500InternalServerError, "An internal service error occurred."),
            TourLogServiceException         => (StatusCodes.Status500InternalServerError, "An internal error occurred."),
            _                               => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };
    }
}
