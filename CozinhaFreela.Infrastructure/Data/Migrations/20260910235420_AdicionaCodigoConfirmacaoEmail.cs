using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozinhaFreela.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCodigoConfirmacaoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodigoConfirmacaoEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CodigoHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConfirmacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tentativas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigoConfirmacaoEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigoConfirmacaoEmail_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodigoConfirmacaoEmail_UsuarioId_DataExpiracao",
                table: "CodigoConfirmacaoEmail",
                columns: new[] { "UsuarioId", "DataExpiracao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigoConfirmacaoEmail");
        }
    }
}
