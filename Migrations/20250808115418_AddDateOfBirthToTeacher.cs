using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIMS_APDP.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfBirthToTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Teachers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Teachers");
        }
    }
}
