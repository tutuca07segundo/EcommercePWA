using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommercePWA.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingCostToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCost",
                table: "Carts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCost",
                table: "Carts");
        }
    }
}
