using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryNoteService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToDemoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UniqueId",
                table: "DemoModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DemoModels_UniqueId",
                table: "DemoModels",
                column: "UniqueId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DemoModels_UniqueId",
                table: "DemoModels");

            migrationBuilder.DropColumn(
                name: "UniqueId",
                table: "DemoModels");
        }
    }
}
