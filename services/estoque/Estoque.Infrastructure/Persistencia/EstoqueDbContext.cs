using Estoque.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Persistencia;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }

    public DbSet<ItemEstoque> ItensEstoque => Set<ItemEstoque>();
    public DbSet<EntradaEstoque> EntradasEstoque => Set<EntradaEstoque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemEstoque>(e =>
        {
            e.ToTable("itens_estoque");
            e.HasKey(i => i.id);
            e.Property(i => i.id).HasColumnName("id");
            e.Property(i => i.produtoId).HasColumnName("produto_id").IsRequired();
            e.HasIndex(i => i.produtoId).IsUnique();
            e.Property(i => i.quantidadeDisponivel).HasColumnName("quantidade_disponivel").IsRequired();
            e.Property(i => i.ultimaAtualizacao).HasColumnName("ultima_atualizacao").IsRequired();
        });

        modelBuilder.Entity<EntradaEstoque>(e =>
        {
            e.ToTable("entradas_estoque");
            e.HasKey(en => en.id);
            e.Property(en => en.id).HasColumnName("id");
            e.Property(en => en.produtoId).HasColumnName("produto_id").IsRequired();
            e.Property(en => en.quantidade).HasColumnName("quantidade").IsRequired();
            e.Property(en => en.numeroNotaFiscal).HasColumnName("numero_nota_fiscal")
                .HasMaxLength(100).IsRequired();
            e.Property(en => en.dataEntrada).HasColumnName("data_entrada").IsRequired();
        });
    }
}
