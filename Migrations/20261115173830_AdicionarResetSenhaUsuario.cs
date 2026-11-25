using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarResetSenhaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "reset_senha_expiracao",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reset_senha_token_hash",
                table: "usuarios",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reset_senha_expiracao",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "reset_senha_token_hash",
                table: "usuarios");
        }
    }
}
