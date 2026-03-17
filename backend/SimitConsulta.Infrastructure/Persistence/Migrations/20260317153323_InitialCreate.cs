using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimitConsulta.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlateQueries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ConsultedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QueryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinesCount = table.Column<int>(type: "int", nullable: false),
                    SummonsCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateQueries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FineDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlateQueryId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InfractionDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Agency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FineDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FineDetails_PlateQueries_PlateQueryId",
                        column: x => x.PlateQueryId,
                        principalTable: "PlateQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SummonsDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlateQueryId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InfractionDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Infraction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummonsDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummonsDetails_PlateQueries_PlateQueryId",
                        column: x => x.PlateQueryId,
                        principalTable: "PlateQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FineDetails_PlateQueryId",
                table: "FineDetails",
                column: "PlateQueryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateQueries_ConsultedAt",
                table: "PlateQueries",
                column: "ConsultedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlateQueries_Plate",
                table: "PlateQueries",
                column: "Plate");

            migrationBuilder.CreateIndex(
                name: "IX_SummonsDetails_PlateQueryId",
                table: "SummonsDetails",
                column: "PlateQueryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FineDetails");

            migrationBuilder.DropTable(
                name: "SummonsDetails");

            migrationBuilder.DropTable(
                name: "PlateQueries");
        }
    }
}
