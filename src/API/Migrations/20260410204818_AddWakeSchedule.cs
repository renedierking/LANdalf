using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddWakeSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WakeSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PcDeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledTime = table.Column<string>(type: "TEXT", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastExecuted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextExecution = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CronExpression = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WakeSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WakeSchedules_PcDevices_PcDeviceId",
                        column: x => x.PcDeviceId,
                        principalTable: "PcDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WakeSchedules_PcDeviceId",
                table: "WakeSchedules",
                column: "PcDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WakeSchedules");
        }
    }
}
