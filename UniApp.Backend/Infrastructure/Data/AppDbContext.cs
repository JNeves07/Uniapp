using Microsoft.EntityFrameworkCore;
using UniApp.Api.Domain.Entities;

namespace UniApp.Api.Infrastructure.Data;

/// <summary>
/// Contexto EF Core. O schema e criado via migracoes (nao EnsureCreated).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Material> Materiais => Set<Material>();
    public DbSet<GrupoEstudo> GruposEstudo => Set<GrupoEstudo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasIndex(u => u.EmailInstitucional).IsUnique();
            entity.Property(u => u.Reputacao).HasPrecision(3, 2);
            entity.Property(u => u.Curso).HasDefaultValue("Análise e Desenvolvimento de Sistemas");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("Materiais");
            entity.Property(m => m.Preco).HasColumnType("decimal(12,2)");
            entity.HasOne(m => m.Usuario)
                  .WithMany(u => u.Materiais)
                  .HasForeignKey(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => m.DataCriacao);
        });

        modelBuilder.Entity<GrupoEstudo>(entity =>
        {
            entity.ToTable("GruposEstudo");
            entity.Property(g => g.Curso).HasDefaultValue("ADS");
        });
    }
}
