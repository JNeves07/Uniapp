using FluentValidation;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;
using UniApp.Api.Application.Mapping;
using UniApp.Api.Application.Validation;
using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Exceptions;
using UniApp.Api.Domain.Rules;

namespace UniApp.Api.Application.Services;

public sealed class GrupoService(
    IGrupoRepository repository,
    IValidator<CriarGrupoRequest> validator) : IGrupoService
{
    public async Task<List<GrupoResponse>> ListAsync(string? formato, string? busca, CancellationToken cancellationToken = default)
    {
        var grupos = await repository.ListAsync(formato, busca, cancellationToken);
        return grupos.Select(DtoMapper.ToResponse).ToList();
    }

    public async Task<GrupoResponse> CriarAsync(CriarGrupoRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var grupo = new GrupoEstudo
        {
            Nome = request.Nome,
            Disciplina = request.Disciplina,
            Curso = request.Curso ?? "ADS",
            Horario = request.Horario ?? "Quintas, 19h",
            Formato = request.Formato ?? "Online",
            VagasDisponiveis = request.VagasDisponiveis ?? 5,
            Recomendado = request.Recomendado ?? true
        };

        GrupoRules.Validate(grupo.Nome, grupo.Disciplina, grupo.Formato, grupo.VagasDisponiveis);
        GrupoRules.Apply(grupo);

        await repository.AddAsync(grupo, cancellationToken);
        return DtoMapper.ToResponse(grupo);
    }

    public async Task<GrupoResponse> ParticiparAsync(int id, CancellationToken cancellationToken = default)
    {
        var grupo = await repository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException("Grupo de estudo não encontrado.");

        GrupoRules.ReservarVaga(grupo);
        await repository.SaveChangesAsync(cancellationToken);

        return DtoMapper.ToResponse(grupo);
    }
}
