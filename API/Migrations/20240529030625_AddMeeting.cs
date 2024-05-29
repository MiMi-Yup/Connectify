using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddMeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Connections_ConnectionId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_ConnectionId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "Contacts");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "Connections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Connections_ContactId",
                table: "Connections",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_Contacts_ContactId",
                table: "Connections",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Connections_Contacts_ContactId",
                table: "Connections");

            migrationBuilder.DropIndex(
                name: "IX_Connections_ContactId",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Connections");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "Contacts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ConnectionId",
                table: "Contacts",
                column: "ConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Connections_ConnectionId",
                table: "Contacts",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "ConnectionId");
        }
    }
}
