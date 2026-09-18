using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CozinhaFreela.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaExpiracaoConfirmacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataExpiracaoConfirmacao",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataExpiracaoConfirmacao",
                table: "AspNetUsers");
        }
    }
}
