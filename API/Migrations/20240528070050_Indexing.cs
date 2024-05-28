using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Indexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Messages_IsDeleted",
                table: "Messages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_IsDeleted",
                table: "Groups",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_IsDeleted",
                table: "GroupMembers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IsDeleted",
                table: "Contacts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted",
                table: "AspNetUsers",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_IsDeleted",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Groups_IsDeleted",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_IsDeleted",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_IsDeleted",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsDeleted",
                table: "AspNetUsers");
        }
    }
}
