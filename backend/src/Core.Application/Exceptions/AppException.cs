namespace Core.Application.Exceptions;

public enum AppErrorKind
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    Unprocessable,
    ExternalService,
}

/// <summary>Erro previsto pelas regras do sistema. O Core.Web converte o tipo em status HTTP.</summary>
public class AppException(AppErrorKind kind, string message) : Exception(message)
{
    public AppErrorKind Kind { get; } = kind;

    public static AppException Validation(string message) => new(AppErrorKind.Validation, message);
    public static AppException Unauthorized(string message = "Faça login para continuar.") => new(AppErrorKind.Unauthorized, message);
    public static AppException Forbidden(string message = "Você não tem acesso a esta loja.") => new(AppErrorKind.Forbidden, message);
    public static AppException NotFound(string message) => new(AppErrorKind.NotFound, message);
    public static AppException Conflict(string message) => new(AppErrorKind.Conflict, message);
    public static AppException Unprocessable(string message) => new(AppErrorKind.Unprocessable, message);
    public static AppException External(string message) => new(AppErrorKind.ExternalService, message);
}
