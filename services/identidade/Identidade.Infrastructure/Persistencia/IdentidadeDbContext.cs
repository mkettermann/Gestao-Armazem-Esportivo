using Identidade.Domain.Entidades;
using Identidade.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Identidade.Infrastructure.Persistencia;

public class IdentidadeDbContext : DbContext
{
    public IdentidadeDbContext(DbContextOptions<IdentidadeDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(u => u.id);
            e.Property(u => u.id).HasColumnName("id");
            e.Property(u => u.nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
            e.Property(u => u.email)
                .HasColumnName("email")
                .HasMaxLength(300)
                .IsRequired()
                .HasConversion(
                    v => v.valor,
                    v => Email.criar(v));
            e.HasIndex(u => u.email).IsUnique();
            e.Property(u => u.senhaHash).HasColumnName("senha_hash").IsRequired();
            e.Property(u => u.tipoUsuario).HasColumnName("tipo_usuario").IsRequired();
            e.Property(u => u.dataCadastro).HasColumnName("data_cadastro").IsRequired();
            e.Property(u => u.ativo).HasColumnName("ativo").IsRequired();
        });
    }
}
