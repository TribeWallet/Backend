using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaIntegrantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "integrantes",
                columns: table => new
                {
                    integrante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integrantes", x => x.integrante_id);
                    table.ForeignKey(
                        name: "fk_integrantes_grupos_grupo_id",
                        column: x => x.grupo_id,
                        principalTable: "grupos",
                        principalColumn: "grupo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_integrantes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_grupo_id",
                table: "integrantes",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_usuario_id_grupo_id",
                table: "integrantes",
                columns: new[] { "usuario_id", "grupo_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integrantes");
        }
    }
}
