using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaCompromissosFinanceiros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compromissos_financeiros",
                columns: table => new
                {
                    compromisso_financeiro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_divisao = table.Column<int>(type: "integer", nullable: false),
                    imagem = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    categoria = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compromissos_financeiros", x => x.compromisso_financeiro_id);
                    table.ForeignKey(
                        name: "fk_compromissos_financeiros_grupos_grupo_id",
                        column: x => x.grupo_id,
                        principalTable: "grupos",
                        principalColumn: "grupo_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_compromissos_financeiros_data",
                table: "compromissos_financeiros",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "ix_compromissos_financeiros_grupo_id",
                table: "compromissos_financeiros",
                column: "grupo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compromissos_financeiros");
        }
    }
}
