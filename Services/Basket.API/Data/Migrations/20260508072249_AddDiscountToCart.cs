using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Basket.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "ShoppingCarts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "ShoppingCarts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "ShoppingCarts");
        }
    }
}
