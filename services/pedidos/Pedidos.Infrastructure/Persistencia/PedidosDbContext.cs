using Pedidos.Domain.Entidades;
using Pedidos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Infrastructure.Persistencia;

public class PedidosDbContext : DbContext
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options) { }

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>(e =>
        {
            e.ToTable("pedidos");
            e.HasKey(p => p.id);
            e.Property(p => p.id).HasColumnName("id");
            e.Property(p => p.documentoCliente)
                .HasColumnName("documento_cliente")
                .HasMaxLength(50)
                .IsRequired()
                .HasConversion(
                    v => v.valor,
                    v => DocumentoCliente.criar(v));
            e.Property(p => p.nomeVendedor).HasColumnName("nome_vendedor")
                .HasMaxLength(200).IsRequired();
            e.Property(p => p.status).HasColumnName("status").IsRequired();
            e.Property(p => p.dataCriacao).HasColumnName("data_criacao").IsRequired();
            e.Property(p => p.dataConfirmacao).HasColumnName("data_confirmacao");
            e.Property(p => p.dataCancelamento).HasColumnName("data_cancelamento");
            e.HasMany(p => p.itens).WithOne()
                .HasForeignKey(i => i.pedidoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemPedido>(e =>
        {
            e.ToTable("itens_pedido");
            e.HasKey(i => i.id);
            e.Property(i => i.id).HasColumnName("id");
            e.Property(i => i.pedidoId).HasColumnName("pedido_id").IsRequired();
            e.Property(i => i.produtoId).HasColumnName("produto_id").IsRequired();
            e.Property(i => i.nomeProduto).HasColumnName("nome_produto")
                .HasMaxLength(300).IsRequired();
            e.Property(i => i.precoUnitario).HasColumnName("preco_unitario")
                .HasPrecision(18, 2).IsRequired();
            e.Property(i => i.quantidade).HasColumnName("quantidade").IsRequired();
            e.Ignore(i => i.subtotal);
        });
    }
}
