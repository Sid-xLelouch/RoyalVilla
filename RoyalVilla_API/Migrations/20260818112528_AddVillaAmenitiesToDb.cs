using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalVilla_API.Migrations
{
    /// <inheritdoc />
    public partial class AddVillaAmenitiesToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE Name = N'CreatedAt'
                      AND Object_ID = Object_ID(N'[dbo].[Users]')
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE Name = N'CreatedDate'
                      AND Object_ID = Object_ID(N'[dbo].[Users]')
                )
                BEGIN
                    EXEC sp_rename N'[dbo].[Users].[CreatedAt]', N'CreatedDate', 'COLUMN';
                END
                """);

            migrationBuilder.CreateTable(
                name: "VillaAmenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VillaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VillaAmenities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VillaAmenities_Villa_VillaId",
                        column: x => x.VillaId,
                        principalTable: "Villa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VillaAmenities_VillaId",
                table: "VillaAmenities",
                column: "VillaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VillaAmenities");

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE Name = N'CreatedDate'
                      AND Object_ID = Object_ID(N'[dbo].[Users]')
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE Name = N'CreatedAt'
                      AND Object_ID = Object_ID(N'[dbo].[Users]')
                )
                BEGIN
                    EXEC sp_rename N'[dbo].[Users].[CreatedDate]', N'CreatedAt', 'COLUMN';
                END
                """);
        }
    }
}
