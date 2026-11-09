using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryNoteService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtendDemoModelWithScheduledJobCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:hun_collation", "hu-HU,hu-HU,icu,True");

            migrationBuilder.AddColumn<int>(
                name: "ClusteredJobCounter",
                table: "DemoModels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LocalJobCounter",
                table: "DemoModels",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClusteredJobCounter",
                table: "DemoModels");

            migrationBuilder.DropColumn(
                name: "LocalJobCounter",
                table: "DemoModels");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:CollationDefinition:hun_collation", "hu-HU,hu-HU,icu,True");
        }
    }
}
