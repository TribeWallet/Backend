using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaNotificacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificacoes",
                columns: table => new
                {
                    notificacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    mensagem = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    data_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacoes", x => x.notificacao_id);
                    table.ForeignKey(
                        name: "fk_notificacoes_usuarios_usuario_destino_id",
                        column: x => x.usuario_destino_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notificacoes_usuarios_usuario_origem_id",
                        column: x => x.usuario_origem_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_entidade_entidade_id",
                table: "notificacoes",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_usuario_destino_id_lida",
                table: "notificacoes",
                columns: new[] { "usuario_destino_id", "lida" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_usuario_origem_id",
                table: "notificacoes",
                column: "usuario_origem_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificacoes");
        }
    }
}
