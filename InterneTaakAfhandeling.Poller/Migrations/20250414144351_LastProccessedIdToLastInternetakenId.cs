using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterneTaakAfhandeling.Poller.Migrations
{
    /// <inheritdoc />
    public partial class LastProccessedIdToLastInternetakenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastProcessedId",
                table: "InternetakenNotifierStates");

            migrationBuilder.AddColumn<Guid>(
                name: "LastInternetakenId",
                table: "InternetakenNotifierStates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastInternetakenId",
                table: "InternetakenNotifierStates");

            migrationBuilder.AddColumn<long>(
                name: "LastProcessedId",
                table: "InternetakenNotifierStates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
