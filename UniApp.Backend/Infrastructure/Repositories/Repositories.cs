using Microsoft.EntityFrameworkCore;
using UniApp.Api.Application.Interfaces;
using UniApp.Api.Domain.Entities;
using UniApp.Api.Infrastructure.Data;

namespace UniApp.Api.Infrastructure.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.EmailInstitucional == email, cancellationToken);

    public Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default)
        => context.Usuarios.AnyAsync(u => u.EmailInstitucional == email, cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GrupoRepository(AppDbContext context) : IGrupoRepository
{
    public async Task<List<GrupoEstudo>> ListAsync(string? formato, string? busca, CancellationToken cancellationToken = default)
    {
        var query = context.GruposEstudo.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(formato) && formato != "Todas")
            query = query.Where(g => g.Formato == formato);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(g => g.Nome.Contains(busca) || g.Disciplina.Contains(busca));

        return await query.OrderBy(g => g.Id).ToListAsync(cancellationToken);
    }

    public Task<GrupoEstudo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.GruposEstudo.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task AddAsync(GrupoEstudo grupo, CancellationToken cancellationToken = default)
    {
        context.GruposEstudo.Add(grupo);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}

public sealed class MaterialRepository(AppDbContext context) : IMaterialRepository
{
    public async Task<List<Material>> ListAsync(string? categoria, string? busca, CancellationToken cancellationToken = default)
    {
        var query = context.Materiais.AsNoTracking().Include(m => m.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria) && categoria != "Todos")
            query = query.Where(m => m.Categoria == categoria);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(m => m.Titulo.Contains(busca) || m.CursoRelacionado.Contains(busca));

        return await query.OrderByDescending(m => m.DataCriacao).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Material material, CancellationToken cancellationToken = default)
    {
        context.Materiais.Add(material);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
