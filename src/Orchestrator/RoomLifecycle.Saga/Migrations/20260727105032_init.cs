using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomLifecycle.Saga.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomStateData",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CurrentState = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Roomid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoomNumber = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomStateData", x => x.CorrelationId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomStateData");
        }
    }
}
