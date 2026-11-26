using FluentValidation;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Domain.Rules;

namespace UniApp.Api.Application.Validation;

/// <summary>
/// Validacoes de entrada da camada de aplicacao (separadas das regras de negocio do dominio).
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(180)
            .WithMessage("Informe seu e-mail institucional.");
        RuleFor(x => x.Senha).NotEmpty().MaximumLength(200)
            .WithMessage("Informe sua senha.");
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(120)
            .WithMessage("Informe seu nome.");
        RuleFor(x => x.Email).NotEmpty().Must(InstitutionalEmail.IsValid)
            .WithMessage(InstitutionalEmail.InvalidMessage);
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(6)
            .WithMessage("A senha deve ter ao menos 6 caracteres.");
        RuleFor(x => x.Curso).MaximumLength(120);
    }
}

public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public sealed class CriarGrupoRequestValidator : AbstractValidator<CriarGrupoRequest>
{
    public CriarGrupoRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(160).WithMessage("Informe o nome do grupo.");
        RuleFor(x => x.Disciplina).NotEmpty().MaximumLength(160).WithMessage("Informe a disciplina do grupo.");
        RuleFor(x => x.Formato).Must(f => string.IsNullOrWhiteSpace(f) || GrupoRules.Formatos.Contains(f, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Formato inválido. Use Online ou Presencial.");
        RuleFor(x => x.VagasDisponiveis).GreaterThanOrEqualTo(0).When(x => x.VagasDisponiveis.HasValue);
    }
}

public sealed class CriarMaterialRequestValidator : AbstractValidator<CriarMaterialRequest>
{
    public CriarMaterialRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(180).WithMessage("Informe o título do material.");
        RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Informe o usuário responsável pelo anúncio.");
        RuleFor(x => x.Preco).GreaterThanOrEqualTo(0).When(x => x.Preco.HasValue)
            .WithMessage("O preço não pode ser negativo.");
        RuleFor(x => x.Modalidade).Must(m => string.IsNullOrWhiteSpace(m) || MaterialRules.Modalidades.Contains(m, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Modalidade inválida. Use Venda, Troca ou Doação.");
    }
}
