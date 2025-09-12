using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommercePWA.Migrations
{
    /// <inheritdoc />
    public partial class AddProductShippingProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Length",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Products",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Products");
        }
    }
}
