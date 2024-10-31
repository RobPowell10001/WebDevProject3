using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDevProject3.Data.Migrations
{
    /// <inheritdoc />
    public partial class addBio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Actor",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Actor");
        }
    }
}
