using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViolationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Violations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BlockType",
                table: "Violations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PolicyId",
                table: "Violations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "BlockType",
                table: "Violations");

            migrationBuilder.DropColumn(
                name: "PolicyId",
                table: "Violations");
        }
    }
}
