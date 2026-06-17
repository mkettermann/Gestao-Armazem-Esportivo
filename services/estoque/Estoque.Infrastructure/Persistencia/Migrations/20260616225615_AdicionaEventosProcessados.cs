using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estoque.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEventosProcessados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "xmin" é uma coluna de sistema do PostgreSQL (já existe em toda tabela) usada como
            // token de concorrência otimista; é apenas mapeada no modelo, não criada via DDL.
            migrationBuilder.CreateTable(
                name: "eventos_processados",
                columns: table => new
                {
                    id_evento = table.Column<Guid>(type: "uuid", nullable: false),
                    rejeitado = table.Column<bool>(type: "boolean", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_processados", x => x.id_evento);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eventos_processados");
        }
    }
}
