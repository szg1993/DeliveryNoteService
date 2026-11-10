using Common.Core.Exceptions;
using Common.Core.Integration;
using Common.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Mime;

namespace Common.AspNetCore.ErrorHandling;

public class ErrorHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        CorrelationIdAccessor correlationIdAccessor,
        MedusaLogger<ErrorHandlingMiddleware> logger
    )
    {
        try
        {
            await next(context);
        }
        catch (NonSuccessResponseException ex)
        {
            context.Response.StatusCode = (int)ex.StatusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(ex.ResponseBody ?? string.Empty);

            LogNonSuccessResponseException(logger, ex);
        }
        catch (Exception ex)
        {
            var logParams = LogParams.Create("requestUrl", context.Request.GetDisplayUrl());
            int statusCode = GetStatusCode(ex);

            if (ex is BadHttpRequestException)
            {
                ex.SetErrorCode(CommonWebErrorCodes.InvalidHttpRequest);
                // Log it because it is not our exception, we did not log it.
                logger.LogTechnical(LogLevel.Warning, ex, "Failed to accept incoming http request.", logParams);
            }

            if (statusCode == StatusCodes.Status500InternalServerError)
                logger.LogTechnical(LogLevel.Error, ex, "An unhandled exception occurred", logParams);

            MedusaErrorResponse error = new(ex)
            {
                CorrelationId = correlationIdAccessor.CorrelationId
            };
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsJsonAsync(error);
        }
    }

    private static int GetStatusCode(Exception ex)
    {
        return ex switch
        {
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            BatchProcessingFailedException => StatusCodes.Status422UnprocessableEntity,
            DataValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            AuthenticationException => StatusCodes.Status401Unauthorized,
            AuthorizationException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
    }

    private static void LogNonSuccessResponseException(MedusaLogger<ErrorHandlingMiddleware> logger, NonSuccessResponseException ex)
    {
        var logParams = new LogParams()
            .AddParam(ex.Url)
            .AddParam(ex.ResponseBody);

        if (ex.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            logger.LogTechnical(LogLevel.Error, ex, "Remote service replied with internal server error.", logParams);
        else
            logger.LogTechnical(LogLevel.Information, ex, "Remote service rejected the request.", logParams);
    }
}