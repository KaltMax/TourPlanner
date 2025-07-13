using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlanner.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tourplanner");

            migrationBuilder.CreateTable(
                name: "Tours",
                schema: "tourplanner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    From = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    To = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GeoJson = table.Column<string>(type: "text", nullable: false),
                    Directions = table.Column<string>(type: "text", nullable: false),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    EstimatedTime = table.Column<double>(type: "double precision", nullable: false),
                    TransportType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TourLogs",
                schema: "tourplanner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TotalDistance = table.Column<double>(type: "double precision", nullable: false),
                    TotalTime = table.Column<double>(type: "double precision", nullable: false),
                    Difficulty = table.Column<double>(type: "double precision", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    TourId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourLogs_Tours_TourId",
                        column: x => x.TourId,
                        principalSchema: "tourplanner",
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourLogs_TourId",
                schema: "tourplanner",
                table: "TourLogs",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_Name",
                schema: "tourplanner",
                table: "Tours",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourLogs",
                schema: "tourplanner");

            migrationBuilder.DropTable(
                name: "Tours",
                schema: "tourplanner");
        }
    }
}
