namespace UniApp.Api.Domain.Exceptions;

/// <summary>
/// Violação de uma regra de negócio. O middleware de erro converte em ProblemDetails 400.
/// </summary>
public class DomainException(string message) : Exception(message);

/// <summary>
/// Recurso solicitado não existe. Convertido em ProblemDetails 404.
/// </summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>
/// Credenciais inválidas ou sessão encerrada. Convertido em ProblemDetails 401.
/// </summary>
public class UnauthorizedException(string message) : Exception(message);

/// <summary>
/// Conflito de estado (por exemplo, e-mail já cadastrado). Convertido em ProblemDetails 409.
/// </summary>
public class ConflictException(string message) : Exception(message);
