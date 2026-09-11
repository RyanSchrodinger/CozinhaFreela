using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozinhaFreela.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrigeFuncaoOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodigoConfirmacaoEmail_AspNetUsers_UsuarioId",
                table: "CodigoConfirmacaoEmail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodigoConfirmacaoEmail",
                table: "CodigoConfirmacaoEmail");

            migrationBuilder.RenameTable(
                name: "CodigoConfirmacaoEmail",
                newName: "CodigosConfirmacaoEmail");

            migrationBuilder.RenameIndex(
                name: "IX_CodigoConfirmacaoEmail_UsuarioId_DataExpiracao",
                table: "CodigosConfirmacaoEmail",
                newName: "IX_CodigosConfirmacaoEmail_UsuarioId_DataExpiracao");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodigosConfirmacaoEmail",
                table: "CodigosConfirmacaoEmail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CodigosConfirmacaoEmail_AspNetUsers_UsuarioId",
                table: "CodigosConfirmacaoEmail",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodigosConfirmacaoEmail_AspNetUsers_UsuarioId",
                table: "CodigosConfirmacaoEmail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodigosConfirmacaoEmail",
                table: "CodigosConfirmacaoEmail");

            migrationBuilder.RenameTable(
                name: "CodigosConfirmacaoEmail",
                newName: "CodigoConfirmacaoEmail");

            migrationBuilder.RenameIndex(
                name: "IX_CodigosConfirmacaoEmail_UsuarioId_DataExpiracao",
                table: "CodigoConfirmacaoEmail",
                newName: "IX_CodigoConfirmacaoEmail_UsuarioId_DataExpiracao");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodigoConfirmacaoEmail",
                table: "CodigoConfirmacaoEmail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CodigoConfirmacaoEmail_AspNetUsers_UsuarioId",
                table: "CodigoConfirmacaoEmail",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
