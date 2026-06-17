using Pedidos.Domain.Entidades;
using Pedidos.Domain.ValueObjects;
using Pedidos.Infrastructure.Persistencia.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Infrastructure.Persistencia;

public class PedidosDbContext : DbContext
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options) { }

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<MensagemOutbox> MensagensOutbox => Set<MensagemOutbox>();

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
            e.Property(p => p.motivoRejeicao).HasColumnName("motivo_rejeicao").HasMaxLength(500);
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

        modelBuilder.Entity<MensagemOutbox>(e =>
        {
            e.ToTable("mensagens_outbox");
            e.HasKey(m => m.id);
            e.Property(m => m.id).HasColumnName("id");
            e.Property(m => m.tipo).HasColumnName("tipo").HasMaxLength(200).IsRequired();
            e.Property(m => m.conteudo).HasColumnName("conteudo").IsRequired();
            e.Property(m => m.exchange).HasColumnName("exchange").HasMaxLength(200).IsRequired();
            e.Property(m => m.routingKey).HasColumnName("routing_key").HasMaxLength(200).IsRequired();
            e.Property(m => m.ocorridoEm).HasColumnName("ocorrido_em").IsRequired();
            e.Property(m => m.processadoEm).HasColumnName("processado_em");
            e.HasIndex(m => m.processadoEm);
        });
    }
}
