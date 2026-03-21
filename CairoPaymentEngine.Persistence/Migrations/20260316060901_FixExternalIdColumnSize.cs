using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CairoPaymentEngine.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixExternalIdColumnSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_ExternalId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Payments",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalId",
                table: "Payments",
                column: "ExternalId",
                unique: true);
        }
    }
}
