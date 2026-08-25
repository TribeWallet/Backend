using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaIntegrantesCompromissos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "integrantes_compromissos",
                columns: table => new
                {
                    integrante_compromisso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integrante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compromisso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_devedor = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integrantes_compromissos", x => x.integrante_compromisso_id);
                    table.ForeignKey(
                        name: "fk_integrantes_compromissos_compromissos_financeiros_compromis",
                        column: x => x.compromisso_id,
                        principalTable: "compromissos_financeiros",
                        principalColumn: "compromisso_financeiro_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_integrantes_compromissos_integrantes_integrante_id",
                        column: x => x.integrante_id,
                        principalTable: "integrantes",
                        principalColumn: "integrante_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_compromissos_compromisso_id",
                table: "integrantes_compromissos",
                column: "compromisso_id");

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_compromissos_integrante_id_compromisso_id",
                table: "integrantes_compromissos",
                columns: new[] { "integrante_id", "compromisso_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integrantes_compromissos");
        }
    }
}
