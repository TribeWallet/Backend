using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <inheritdoc />
    public partial class CriarSchemaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    token = table.Column<string>(type: "character(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grupos", x => x.grupo_id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sobrenome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    imagem = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    hash_senha = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    token = table.Column<string>(type: "character(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.usuario_id);
                });

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
                    categoria = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    token = table.Column<string>(type: "character(64)", nullable: false)
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
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    token = table.Column<string>(type: "character(64)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "integrantes",
                columns: table => new
                {
                    integrante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character(64)", nullable: false)
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
                    lida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    token = table.Column<string>(type: "character(64)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "relatorios",
                columns: table => new
                {
                    relatorio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compromisso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    conteudo_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    token = table.Column<string>(type: "character(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relatorios", x => x.relatorio_id);
                    table.ForeignKey(
                        name: "fk_relatorios_compromissos_financeiros_compromisso_id",
                        column: x => x.compromisso_id,
                        principalTable: "compromissos_financeiros",
                        principalColumn: "compromisso_financeiro_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_relatorios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "integrantes_compromissos",
                columns: table => new
                {
                    integrante_compromisso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integrante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    compromisso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_devedor = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, defaultValue: 0m),
                    token = table.Column<string>(type: "character(64)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    pagamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integrante_compromisso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    comprovante_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    metodo = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character(64)", nullable: false)
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
                name: "ix_compromissos_financeiros_data",
                table: "compromissos_financeiros",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "ix_compromissos_financeiros_grupo_id",
                table: "compromissos_financeiros",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_compromissos_financeiros_token",
                table: "compromissos_financeiros",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_grupos_token",
                table: "grupos",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_data_hora",
                table: "historico_alteracoes",
                column: "data_hora");

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_entidade_entidade_id",
                table: "historico_alteracoes",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_token",
                table: "historico_alteracoes",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historico_alteracoes_usuario_id",
                table: "historico_alteracoes",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_grupo_id",
                table: "integrantes",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_token",
                table: "integrantes",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_usuario_id_grupo_id",
                table: "integrantes",
                columns: new[] { "usuario_id", "grupo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_compromissos_compromisso_id",
                table: "integrantes_compromissos",
                column: "compromisso_id");

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_compromissos_integrante_id_compromisso_id",
                table: "integrantes_compromissos",
                columns: new[] { "integrante_id", "compromisso_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_integrantes_compromissos_token",
                table: "integrantes_compromissos",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_entidade_entidade_id",
                table: "notificacoes",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_token",
                table: "notificacoes",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_usuario_destino_id_lida",
                table: "notificacoes",
                columns: new[] { "usuario_destino_id", "lida" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_usuario_origem_id",
                table: "notificacoes",
                column: "usuario_origem_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_data",
                table: "pagamentos",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_integrante_compromisso_id",
                table: "pagamentos",
                column: "integrante_compromisso_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_token",
                table: "pagamentos",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_compromisso_id",
                table: "relatorios",
                column: "compromisso_id");

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_token",
                table: "relatorios",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_usuario_id",
                table: "relatorios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_token",
                table: "usuarios",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_username",
                table: "usuarios",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historico_alteracoes");

            migrationBuilder.DropTable(
                name: "notificacoes");

            migrationBuilder.DropTable(
                name: "pagamentos");

            migrationBuilder.DropTable(
                name: "relatorios");

            migrationBuilder.DropTable(
                name: "integrantes_compromissos");

            migrationBuilder.DropTable(
                name: "compromissos_financeiros");

            migrationBuilder.DropTable(
                name: "integrantes");

            migrationBuilder.DropTable(
                name: "grupos");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
