using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommercePWA.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Carts",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Carts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Carts");
        }
    }
}
