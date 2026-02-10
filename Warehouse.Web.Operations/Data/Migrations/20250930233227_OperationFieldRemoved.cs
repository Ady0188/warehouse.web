using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Web.Operations.Data.Migrations
{
    /// <inheritdoc />
    public partial class OperationFieldRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Paid",
                schema: "Operations",
                table: "Operations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Paid",
                schema: "Operations",
                table: "Operations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
