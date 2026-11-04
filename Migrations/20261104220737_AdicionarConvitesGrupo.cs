using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarConvitesGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "convites_grupo",
                columns: table => new
                {
                    convite_grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_convidado = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    token = table.Column<string>(type: "character(64)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_convites_grupo", x => x.convite_grupo_id);
                    table.ForeignKey(
                        name: "fk_convites_grupo_grupos_grupo_id",
                        column: x => x.grupo_id,
                        principalTable: "grupos",
                        principalColumn: "grupo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_convites_grupo_usuarios_criado_por_usuario_id",
                        column: x => x.criado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_convites_grupo_criado_por_usuario_id",
                table: "convites_grupo",
                column: "criado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_convites_grupo_deleted_at",
                table: "convites_grupo",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_convites_grupo_grupo_id",
                table: "convites_grupo",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_convites_grupo_token",
                table: "convites_grupo",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "convites_grupo");
        }
    }
}
