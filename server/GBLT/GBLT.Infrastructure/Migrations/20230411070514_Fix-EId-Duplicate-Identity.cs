using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GBLT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEIdDuplicateIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TRefreshToken_User_TUserId",
                table: "TRefreshToken");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "User",
                newName: "EId");

            migrationBuilder.RenameColumn(
                name: "TUserId",
                table: "TRefreshToken",
                newName: "TUserEId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TRefreshToken",
                newName: "EId");

            migrationBuilder.RenameIndex(
                name: "IX_TRefreshToken_TUserId",
                table: "TRefreshToken",
                newName: "IX_TRefreshToken_TUserEId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Meta",
                newName: "EId");

            migrationBuilder.AddForeignKey(
                name: "FK_TRefreshToken_User_TUserEId",
                table: "TRefreshToken",
                column: "TUserEId",
                principalTable: "User",
                principalColumn: "EId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TRefreshToken_User_TUserEId",
                table: "TRefreshToken");

            migrationBuilder.RenameColumn(
                name: "EId",
                table: "User",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TUserEId",
                table: "TRefreshToken",
                newName: "TUserId");

            migrationBuilder.RenameColumn(
                name: "EId",
                table: "TRefreshToken",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_TRefreshToken_TUserEId",
                table: "TRefreshToken",
                newName: "IX_TRefreshToken_TUserId");

            migrationBuilder.RenameColumn(
                name: "EId",
                table: "Meta",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TRefreshToken_User_TUserId",
                table: "TRefreshToken",
                column: "TUserId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
