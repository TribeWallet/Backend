using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEntityArquivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "arquivos",
                columns: table => new
                {
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    token = table.Column<string>(type: "character(64)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_arquivos", x => x.arquivo_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_arquivos_deleted_at",
                table: "arquivos",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_arquivos_token",
                table: "arquivos",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "arquivos");
        }
    }
}
