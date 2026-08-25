using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaPagamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    pagamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integrante_compromisso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    comprovante_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    metodo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamentos", x => x.pagamento_id);
                    table.ForeignKey(
                        name: "fk_pagamentos_integrantes_compromissos_integrante_compromisso_",
                        column: x => x.integrante_compromisso_id,
                        principalTable: "integrantes_compromissos",
                        principalColumn: "integrante_compromisso_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_data",
                table: "pagamentos",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_integrante_compromisso_id",
                table: "pagamentos",
                column: "integrante_compromisso_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagamentos");
        }
    }
}
