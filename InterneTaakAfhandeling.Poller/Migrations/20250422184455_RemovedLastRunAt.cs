using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterneTaakAfhandeling.Poller.Migrations
{
    /// <inheritdoc />
    public partial class RemovedLastRunAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRunAt",
                table: "InternetakenNotifierStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastRunAt",
                table: "InternetakenNotifierStates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
