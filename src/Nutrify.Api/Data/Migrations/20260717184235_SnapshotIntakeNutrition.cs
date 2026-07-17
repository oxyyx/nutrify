using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nutrify.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotIntakeNutrition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_intake_entries_food_items_FoodItemId",
                table: "intake_entries");

            migrationBuilder.AlterColumn<int>(
                name: "FoodItemId",
                table: "intake_entries",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // Add the snapshot columns as nullable first so existing rows can be
            // backfilled from their (still-present) food item before NOT NULL is
            // enforced below.
            migrationBuilder.AddColumn<string>(
                name: "FoodItemName",
                table: "intake_entries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodItemUnit",
                table: "intake_entries",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CaloriesKcal",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProteinG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarbohydratesG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FatG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FiberG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            // Backfill the snapshot for every existing entry from its linked food
            // item. All pre-existing rows have a non-null FoodItemId (the old FK
            // was required), so this covers the whole table.
            migrationBuilder.Sql("""
                UPDATE intake_entries e
                SET "FoodItemName" = f."Name",
                    "FoodItemUnit" = f."Unit",
                    "CaloriesKcal" = f."CaloriesKcal",
                    "ProteinG" = f."ProteinG",
                    "CarbohydratesG" = f."CarbohydratesG",
                    "FatG" = f."FatG",
                    "FiberG" = f."FiberG"
                FROM food_items f
                WHERE e."FoodItemId" = f."Id";
                """);

            // Now enforce NOT NULL to match the model.
            migrationBuilder.AlterColumn<string>(
                name: "FoodItemName",
                table: "intake_entries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FoodItemUnit",
                table: "intake_entries",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CaloriesKcal",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProteinG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CarbohydratesG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FatG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FiberG",
                table: "intake_entries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_intake_entries_food_items_FoodItemId",
                table: "intake_entries",
                column: "FoodItemId",
                principalTable: "food_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_intake_entries_food_items_FoodItemId",
                table: "intake_entries");

            migrationBuilder.DropColumn(
                name: "CaloriesKcal",
                table: "intake_entries");

            migrationBuilder.DropColumn(
                name: "CarbohydratesG",
                table: "intake_entries");

            migrationBuilder.DropColumn(
                name: "FatG",
                table: "intake_entries");

            migrationBuilder.DropColumn(
                name: "FiberG",
                table: "intake_entries");

            migrationBuilder.DropColumn(
                name: "FoodItemName",
                table: "intake_entries");

            migrationBuilder.DropColumn(
                name: "FoodItemUnit",
                table: "intake_entries");

            migrationBuilder.DropColumn(
                name: "ProteinG",
                table: "intake_entries");

            migrationBuilder.AlterColumn<int>(
                name: "FoodItemId",
                table: "intake_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_intake_entries_food_items_FoodItemId",
                table: "intake_entries",
                column: "FoodItemId",
                principalTable: "food_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
