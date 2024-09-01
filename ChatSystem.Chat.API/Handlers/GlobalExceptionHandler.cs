using System.Data.Common;
using System.Net;
using System.Text.Json;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Utils.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ChatSystem.Chat.API.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
            };

            var _currentUserService = httpContext.RequestServices.GetRequiredService<ICurrentUserService>();

            if (exception is AppException)
            {
                var appException = exception as AppException;
                problemDetails.Title = appException!.Error.ErrorKey;
                problemDetails.Detail = appException.Error.Message;
                problemDetails.Status = (int)appException.Error.StatusCode;
                _logger.LogWarning(exception, "Application Error in Custom Middleware");
                httpContext.Response.StatusCode = (int)appException.Error.StatusCode;
            }
            else if (exception is BadHttpRequestException)
            {
                problemDetails.Title = "bad_request";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                _logger.LogError(exception, "A bad request was received");
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            }
            else if (exception is UnauthorizedAccessException)
            {
                problemDetails.Title = "unauthorized_access";
                problemDetails.Detail = exception.Message;
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                _logger.LogError(exception, "Unauthorized exception occurred");
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            }
            else if (exception is DbUpdateException)
            {
                if (exception.InnerException is DbException dbException)
                {
                    problemDetails.Title = "unique_constraint_violation";
                    problemDetails.Detail = "A conflict occured.";
                    problemDetails.Status = (int)HttpStatusCode.Conflict;
                    _logger.LogError(exception, "Db exception occurred");

                    httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                }
                else
                {
                    problemDetails.Title = "dbupdate_failed";
                    problemDetails.Detail = "Db update failed.";
                    problemDetails.Status = (int)HttpStatusCode.ExpectationFailed;

                    httpContext.Response.StatusCode = StatusCodes.Status417ExpectationFailed;

                    _logger.LogError(exception, "DbUpdate exception occurred");
                }
            }
            else
            {
                problemDetails.Title = "something_went_wrong";
                problemDetails.Detail = "Something went wrong";
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                _logger.LogError(exception, "An unhandled exception occurred");
            }

            // Add a trace ID
            var traceId = httpContext.TraceIdentifier;
            problemDetails.Extensions.Add("traceId", traceId);
            problemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";

            // Add exception details only in development or local environment
            if (httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() ||
                string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Local", StringComparison.OrdinalIgnoreCase))
            {
                problemDetails.Extensions.Add("exceptionMessage", exception.Message);
                problemDetails.Extensions.Add("innerException", exception.InnerException?.Message);
                problemDetails.Extensions.Add("stackTrace", exception.StackTrace);

            }

            string responseText = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            await httpContext.Response.WriteAsync(responseText);

            return true;
        }
    }
}
