using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace DeliveryNoteService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite numeric affinity would round stored decimal values.
            var decimalType = ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite" ? "TEXT" : "numeric";

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EkaerNbr = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TakeoverPlace = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Comments = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsUrgent = table.Column<bool>(type: "boolean", nullable: false),
                    IsCrackable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDangerous = table.Column<bool>(type: "boolean", nullable: false),
                    IsOwnCost = table.Column<bool>(type: "boolean", nullable: false),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NoteStatusId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.CheckConstraint("CK_Notes_NumberRange", "\"Id\" >= 1 AND \"Id\" <= 99999");
                    table.ForeignKey(
                        name: "FK_Notes_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "NoteContactInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NoteId = table.Column<int>(type: "integer", nullable: false),
                    AssignTo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssignToPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteContactInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteContactInfo_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "NoteDeadline",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NoteId = table.Column<int>(type: "integer", nullable: false),
                    TakeoverDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShipDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EstimatedArrivalDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteDeadline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteDeadline_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "NoteLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NoteId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    PartCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PartDesc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartCmt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PartQty = table.Column<decimal>(type: decimalType, nullable: false),
                    PartQtyUm = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PartWgt = table.Column<decimal>(type: decimalType, nullable: false),
                    PartWgtUm = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoteLine_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "NotePackageDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NoteId = table.Column<int>(type: "integer", nullable: false),
                    TareWgt = table.Column<decimal>(type: decimalType, nullable: false),
                    TareWgtUm = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PkgQty = table.Column<decimal>(type: decimalType, nullable: false),
                    PkgScale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PkgSizeX = table.Column<decimal>(type: decimalType, nullable: false),
                    PkgSizeY = table.Column<decimal>(type: decimalType, nullable: false),
                    PkgSizeZ = table.Column<decimal>(type: decimalType, nullable: false),
                    PkgSizeUm = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotePackageDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotePackageDetail_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_NoteContactInfo_NoteId",
                table: "NoteContactInfo",
                column: "NoteId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_NoteDeadline_NoteId",
                table: "NoteDeadline",
                column: "NoteId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_NoteLine_NoteId_Position",
                table: "NoteLine",
                columns: new[] { "NoteId", "Position" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_NotePackageDetail_NoteId",
                table: "NotePackageDetail",
                column: "NoteId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notes_RecipientId",
                table: "Notes",
                column: "RecipientId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoteContactInfo"
            );

            migrationBuilder.DropTable(
                name: "NoteDeadline"
            );

            migrationBuilder.DropTable(
                name: "NoteLine"
            );

            migrationBuilder.DropTable(
                name: "NotePackageDetail"
            );

            migrationBuilder.DropTable(
                name: "Notes"
            );

        }
    }
}