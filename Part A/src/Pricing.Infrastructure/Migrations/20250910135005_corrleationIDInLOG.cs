using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class corrleationIDInLOG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "Logs");
        }
    }
}
