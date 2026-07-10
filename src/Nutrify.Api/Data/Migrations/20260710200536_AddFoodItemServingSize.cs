using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrify.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodItemServingSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ServingSize",
                table: "food_items",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServingSizeName",
                table: "food_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServingSize",
                table: "food_items");

            migrationBuilder.DropColumn(
                name: "ServingSizeName",
                table: "food_items");
        }
    }
}
