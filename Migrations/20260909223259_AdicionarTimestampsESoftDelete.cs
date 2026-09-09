using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTimestampsESoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "relatorios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "relatorios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "relatorios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "pagamentos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "pagamentos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "pagamentos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "notificacoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "notificacoes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "notificacoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "integrantes_compromissos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "integrantes_compromissos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "integrantes_compromissos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "integrantes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "integrantes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "integrantes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "historico_alteracoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "historico_alteracoes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "historico_alteracoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "grupos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "grupos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "grupos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "compromissos_financeiros",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "compromissos_financeiros",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "compromissos_financeiros",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_deleted_at",
                table: "usuarios",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_deleted_at",
                table: "relatorios",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_deleted_at",
                table: "pagamentos",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_deleted_at",
                table: "notificacoes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_compromissos_deleted_at",
                table: "integrantes_compromissos",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_deleted_at",
                table: "integrantes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_deleted_at",
                table: "historico_alteracoes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_grupos_deleted_at",
                table: "grupos",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_compromissos_financeiros_deleted_at",
                table: "compromissos_financeiros",
                column: "deleted_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuarios_deleted_at",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "ix_relatorios_deleted_at",
                table: "relatorios");

            migrationBuilder.DropIndex(
                name: "ix_pagamentos_deleted_at",
                table: "pagamentos");

            migrationBuilder.DropIndex(
                name: "ix_notificacoes_deleted_at",
                table: "notificacoes");

            migrationBuilder.DropIndex(
                name: "ix_integrantes_compromissos_deleted_at",
                table: "integrantes_compromissos");

            migrationBuilder.DropIndex(
                name: "ix_integrantes_deleted_at",
                table: "integrantes");

            migrationBuilder.DropIndex(
                name: "ix_historico_alteracoes_deleted_at",
                table: "historico_alteracoes");

            migrationBuilder.DropIndex(
                name: "ix_grupos_deleted_at",
                table: "grupos");

            migrationBuilder.DropIndex(
                name: "ix_compromissos_financeiros_deleted_at",
                table: "compromissos_financeiros");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "relatorios");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "relatorios");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "relatorios");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "pagamentos");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "pagamentos");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "pagamentos");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "notificacoes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "notificacoes");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "notificacoes");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "integrantes_compromissos");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "integrantes_compromissos");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "integrantes_compromissos");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "integrantes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "integrantes");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "integrantes");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "historico_alteracoes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "historico_alteracoes");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "historico_alteracoes");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "grupos");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "grupos");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "grupos");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "compromissos_financeiros");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "compromissos_financeiros");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "compromissos_financeiros");
        }
    }
}
