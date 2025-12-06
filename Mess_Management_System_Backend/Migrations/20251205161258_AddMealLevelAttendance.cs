using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mess_Management_System_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMealLevelAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Attendances");

            migrationBuilder.AddColumn<bool>(
                name: "AttendedBreakfast",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendedDinner",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendedLunch",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendedBreakfast",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AttendedDinner",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AttendedLunch",
                table: "Attendances");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
