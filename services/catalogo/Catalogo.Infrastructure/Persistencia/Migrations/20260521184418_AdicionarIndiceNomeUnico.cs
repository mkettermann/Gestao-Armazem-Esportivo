using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogo.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIndiceNomeUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índice de expressão case-insensitive: impede nomes duplicados independente de maiúsculas/minúsculas.
            // Ex.: "Nike Air Max" e "NIKE AIR MAX" são considerados iguais.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ix_produtos_nome_lower ON produtos (LOWER(nome)) WHERE ativo = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_produtos_nome_lower;");
        }
    }
}
