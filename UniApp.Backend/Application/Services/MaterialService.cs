using FluentValidation;
using UniApp.Api.Application.DTOs;
using UniApp.Api.Application.Interfaces;
using UniApp.Api.Application.Mapping;
using UniApp.Api.Application.Validation;
using UniApp.Api.Domain.Entities;
using UniApp.Api.Domain.Exceptions;
using UniApp.Api.Domain.Rules;

namespace UniApp.Api.Application.Services;

public sealed class MaterialService(
    IMaterialRepository repository,
    IUsuarioRepository usuarios,
    IValidator<CriarMaterialRequest> validator) : IMaterialService
{
    public async Task<List<MaterialResponse>> ListAsync(string? categoria, string? busca, CancellationToken cancellationToken = default)
    {
        var materiais = await repository.ListAsync(categoria, busca, cancellationToken);
        return materiais.Select(DtoMapper.ToResponse).ToList();
    }

    public async Task<MaterialResponse> CriarAsync(CriarMaterialRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        MaterialRules.Validate(request.Titulo, request.Modalidade ?? "Venda", request.Preco ?? 0);

        var usuario = await usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
                      ?? throw new NotFoundException("Usuário responsável pelo anúncio não encontrado.");

        var material = new Material
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao ?? string.Empty,
            Categoria = string.IsNullOrWhiteSpace(request.Categoria) ? "Livros" : request.Categoria,
            CursoRelacionado = string.IsNullOrWhiteSpace(request.CursoRelacionado) ? "ADS" : request.CursoRelacionado,
            EstadoConservacao = string.IsNullOrWhiteSpace(request.EstadoConservacao) ? "Bom estado" : request.EstadoConservacao,
            Modalidade = request.Modalidade ?? "Venda",
            Preco = request.Preco ?? 0,
            ImagemUrl = request.ImagemUrl ?? new Material().ImagemUrl,
            ContatoWhatsApp = request.ContatoWhatsApp ?? string.Empty,
            UsuarioId = usuario.Id
        };

        MaterialRules.Apply(material);

        await repository.AddAsync(material, cancellationToken);
        return DtoMapper.ToResponse(material) with { Usuario = DtoMapper.ToResponse(usuario) };
    }
}
