using CoreForge.Application.Common.Exceptions;
using CoreForge.Application.Common.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace CoreForge.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IStringLocalizerFactory _localizerFactory;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IStringLocalizerFactory localizerFactory)
    {
        _next = next;
        _logger = logger;
        _localizerFactory = localizerFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            var localizer = _localizerFactory.Create(typeof(SharedResource));
            var message = localizer[ex.ResourceKey].Value;

            _logger.LogWarning("AppException [{StatusCode}]: {ResourceKey} — {Message}",
                ex.StatusCode, ex.ResourceKey, message);

            await WriteProblemDetails(context, ex.StatusCode, message);
        }
        catch (Exception ex)
        {
            var localizer = _localizerFactory.Create(typeof(SharedResource));
            var message = localizer[ResourceKeys.Errors.InternalServer].Value;

            _logger.LogError(ex, "Unhandled exception");

            await WriteProblemDetails(context, 500, message);
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        500 => "Internal Server Error",
        _   => "Error"
    };
}
