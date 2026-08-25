using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaHistoricoAlteracoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "historico_alteracoes",
                columns: table => new
                {
                    historico_alteracao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    dados_antes = table.Column<string>(type: "jsonb", nullable: true),
                    dados_depois = table.Column<string>(type: "jsonb", nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historico_alteracoes", x => x.historico_alteracao_id);
                    table.ForeignKey(
                        name: "fk_historico_alteracoes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_data_hora",
                table: "historico_alteracoes",
                column: "data_hora");

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_entidade_entidade_id",
                table: "historico_alteracoes",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_usuario_id",
                table: "historico_alteracoes",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historico_alteracoes");
        }
    }
}
