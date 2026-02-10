using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Web.Operations.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewFieldAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Difference",
                schema: "Operations",
                table: "Product",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difference",
                schema: "Operations",
                table: "Product");
        }
    }
}
