using UniApp.Api.Application.DTOs;
using UniApp.Api.Domain.Entities;

namespace UniApp.Api.Application.Mapping;

/// <summary>
/// Conversao entre entidades de dominio e DTOs. Nunca expoe SenhaHash.
/// </summary>
public static class DtoMapper
{
    public static UsuarioResponse ToResponse(Usuario usuario) => new(
        usuario.Id,
        usuario.Nome,
        usuario.EmailInstitucional,
        usuario.Curso,
        usuario.Reputacao,
        usuario.AvaliacoesCount);

    public static GrupoResponse ToResponse(GrupoEstudo grupo) => new(
        grupo.Id,
        grupo.Nome,
        grupo.Disciplina,
        grupo.Curso,
        grupo.Horario,
        grupo.Formato,
        grupo.VagasDisponiveis,
        grupo.Recomendado);

    public static MaterialResponse ToResponse(Material material) => new(
        material.Id,
        material.Titulo,
        material.Descricao,
        material.Categoria,
        material.CursoRelacionado,
        material.EstadoConservacao,
        material.Modalidade,
        material.Preco,
        material.ImagemUrl,
        material.ContatoWhatsApp,
        material.DataCriacao,
        material.Usuario is null ? null : ToResponse(material.Usuario));
}
