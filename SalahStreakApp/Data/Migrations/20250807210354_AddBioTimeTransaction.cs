using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalahStreakApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBioTimeTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BioTimeTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemoteId = table.Column<long>(type: "INTEGER", nullable: false),
                    Emp = table.Column<int>(type: "INTEGER", nullable: true),
                    EmpCode = table.Column<string>(type: "TEXT", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Department = table.Column<string>(type: "TEXT", nullable: true),
                    Position = table.Column<string>(type: "TEXT", nullable: true),
                    PunchTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PunchState = table.Column<string>(type: "TEXT", nullable: true),
                    PunchStateDisplay = table.Column<string>(type: "TEXT", nullable: true),
                    VerifyType = table.Column<int>(type: "INTEGER", nullable: true),
                    VerifyTypeDisplay = table.Column<string>(type: "TEXT", nullable: true),
                    WorkCode = table.Column<string>(type: "TEXT", nullable: true),
                    GpsLocation = table.Column<string>(type: "TEXT", nullable: true),
                    AreaAlias = table.Column<string>(type: "TEXT", nullable: true),
                    TerminalSn = table.Column<string>(type: "TEXT", nullable: true),
                    Temperature = table.Column<double>(type: "REAL", nullable: true),
                    IsMask = table.Column<string>(type: "TEXT", nullable: true),
                    TerminalAlias = table.Column<string>(type: "TEXT", nullable: true),
                    UploadTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BioTimeTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BioTimeTransactions_RemoteId",
                table: "BioTimeTransactions",
                column: "RemoteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BioTimeTransactions");
        }
    }
}
