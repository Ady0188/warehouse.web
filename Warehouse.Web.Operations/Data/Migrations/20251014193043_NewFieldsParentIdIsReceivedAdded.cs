using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Web.Operations.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewFieldsParentIdIsReceivedAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReceived",
                schema: "Operations",
                table: "Operations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                schema: "Operations",
                table: "Operations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReceived",
                schema: "Operations",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "Operations",
                table: "Operations");
        }
    }
}
