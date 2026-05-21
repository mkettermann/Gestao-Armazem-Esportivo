using Catalogo.Domain.Entidades;
using Catalogo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Persistencia;

public class CatalogoDbContext : DbContext
{
    public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(e =>
        {
            e.ToTable("produtos");
            e.HasKey(p => p.id);
            e.Property(p => p.id).HasColumnName("id");
            e.Property(p => p.nome).HasColumnName("nome").HasMaxLength(300).IsRequired();
            e.Property(p => p.descricao).HasColumnName("descricao").HasMaxLength(2000);
            e.Property(p => p.preco)
                .HasColumnName("preco")
                .HasPrecision(18, 2)
                .IsRequired()
                .HasConversion(
                    v => v.valor,
                    v => Preco.criar(v));
            e.Property(p => p.dataCadastro).HasColumnName("data_cadastro").IsRequired();
            e.Property(p => p.ativo).HasColumnName("ativo").IsRequired();
        });
    }
}
