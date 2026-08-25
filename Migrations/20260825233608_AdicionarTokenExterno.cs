using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TribeWallet.Migrations
{
    /// <summary>
    /// Dá a toda tabela um token público de 64 hex, único, separado do id interno.
    /// A coluna entra anulável, recebe um token sorteado por linha e só depois vira
    /// obrigatória — senão as linhas já existentes colidiriam no índice único.
    /// </summary>
    public partial class AdicionarTokenExterno : Migration
    {
        /// <summary>Toda tabela do schema recebe token; a ordem acompanha a das migrations.</summary>
        private static readonly string[] Tabelas =
        [
            "usuarios",
            "grupos",
            "integrantes",
            "compromissos_financeiros",
            "integrantes_compromissos",
            "pagamentos",
            "relatorios",
            "notificacoes",
            "historico_alteracoes"
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // gen_random_bytes vem do pgcrypto. sha256() é nativo desde o Postgres 11.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            foreach (var tabela in Tabelas)
            {
                migrationBuilder.AddColumn<string>(
                    name: "token",
                    table: tabela,
                    type: "character(64)",
                    nullable: true);

                // Um token distinto por linha, sorteado pelo próprio banco.
                migrationBuilder.Sql(
                    $"UPDATE {tabela} SET token = encode(sha256(gen_random_bytes(32)), 'hex');");

                migrationBuilder.AlterColumn<string>(
                    name: "token",
                    table: tabela,
                    type: "character(64)",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "character(64)",
                    oldNullable: true);

                migrationBuilder.CreateIndex(
                    name: $"ix_{tabela}_token",
                    table: tabela,
                    column: "token",
                    unique: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var tabela in Tabelas)
            {
                migrationBuilder.DropIndex(name: $"ix_{tabela}_token", table: tabela);
                migrationBuilder.DropColumn(name: "token", table: tabela);
            }
        }
    }
}
