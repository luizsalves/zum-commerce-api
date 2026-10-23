using Core.Application.Exceptions;

namespace Core.Web.Middleware;

/// <summary>Converte erros em respostas JSON { error } com o status certo.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            var status = ex.Kind switch
            {
                AppErrorKind.Validation => StatusCodes.Status400BadRequest,
                AppErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
                AppErrorKind.Forbidden => StatusCodes.Status403Forbidden,
                AppErrorKind.NotFound => StatusCodes.Status404NotFound,
                AppErrorKind.Conflict => StatusCodes.Status409Conflict,
                AppErrorKind.Unprocessable => StatusCodes.Status422UnprocessableEntity,
                AppErrorKind.ExternalService => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status500InternalServerError,
            };
            if (status >= 500) logger.LogWarning(ex, "Falha em serviço externo");
            await WriteAsync(context, status, ex.Message);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Cliente desistiu da requisição; nada a responder.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado em {Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "Erro interno. Tente novamente.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}
