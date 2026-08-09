namespace Api_Eden.Configs;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;



public class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ocurrió un error no controlado: {Message}", exception.Message);

        // Personalizamos la respuesta según el tipo de excepción
        // var (statusCode, title) = exception switch
        // {
        //     ArgumentException => (StatusCodes.Status400BadRequest, "Petición incorrecta (Validación)"),
        //     UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado"),
        //     _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        // };

        var (statusCode, title) = exception switch
        {
            ArgumentException        => (400, "Petición incorrecta (Validación)"),
            UnauthorizedAccessException => (401, "No autorizado"),
            KeyNotFoundException      => (404, "Recurso no encontrado"),
            InvalidOperationException => (409, "Operación no permitida"),
            NotImplementedException   => (501, "Funcionalidad no implementada"),
            TimeoutException          => (408, "Tiempo de espera agotado"),
            _                         => (500, "Error interno del servidor")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Indica que la excepción ya fue manejada
    }
}