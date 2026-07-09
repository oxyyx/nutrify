using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrify.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodItemBarcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "food_items",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_food_items_UserId_Barcode",
                table: "food_items",
                columns: new[] { "UserId", "Barcode" },
                unique: true,
                filter: "\"Barcode\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_food_items_UserId_Barcode",
                table: "food_items");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "food_items");
        }
    }
}
