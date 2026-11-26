using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using UniApp.Api.Domain.Exceptions;

namespace UniApp.Api.Middlewares;

/// <summary>
/// Converte excecoes em respostas ProblemDetails consistentes para toda a API.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IProblemDetailsService problemDetailsService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning(ex, "Excecao apos o inicio da resposta: {Message}", ex.Message);
                throw;
            }

            var (status, title, message) = Map(ex);

            if (status >= 500)
                logger.LogError(ex, "Erro nao tratado: {Message}", ex.Message);
            else
                logger.LogWarning(ex, "{Status} {Message}", status, ex.Message);

            context.Response.Clear();
            context.Response.StatusCode = status;

            var problem = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = message,
                Instance = context.Request.Path
            };

            // Extensao "mensagem" mantem compatibilidade com o formato anterior da API.
            problem.Extensions["mensagem"] = message;

            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem
            });
        }
    }

    private static (int Status, string Title, string Message) Map(Exception ex) => ex switch
    {
        DomainException => (StatusCodes.Status400BadRequest, "Regra de negócio violada", ex.Message),
        ValidationException validation =>
            (StatusCodes.Status400BadRequest, "Dados inválidos",
             string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage))),
        UnauthorizedException => (StatusCodes.Status401Unauthorized, "Não autorizado", ex.Message),
        NotFoundException => (StatusCodes.Status404NotFound, "Não encontrado", ex.Message),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requisição inválida", ex.Message),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor",
              "Ocorreu um erro inesperado. Tente novamente mais tarde.")
    };
}
