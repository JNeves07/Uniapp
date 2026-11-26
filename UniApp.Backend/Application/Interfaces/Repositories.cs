using UniApp.Api.Domain.Entities;

namespace UniApp.Api.Application.Interfaces;

/// <summary>
/// Contratos de persistencia da camada de aplicacao. A implementacao vive na Infrastructure.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
}

public interface IGrupoRepository
{
    Task<List<GrupoEstudo>> ListAsync(string? formato, string? busca, CancellationToken cancellationToken = default);
    Task<GrupoEstudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(GrupoEstudo grupo, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IMaterialRepository
{
    Task<List<Material>> ListAsync(string? categoria, string? busca, CancellationToken cancellationToken = default);
    Task AddAsync(Material material, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
