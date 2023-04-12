using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GBLT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizCollectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizCollection",
                columns: table => new
                {
                    EId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Configuration = table.Column<string>(type: "text", nullable: true),
                    OwnerEId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCollection", x => x.EId);
                    table.ForeignKey(
                        name: "FK_QuizCollection_User_OwnerEId",
                        column: x => x.OwnerEId,
                        principalTable: "User",
                        principalColumn: "EId");
                });

            migrationBuilder.CreateTable(
                name: "Quiz",
                columns: table => new
                {
                    EId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Quetsion = table.Column<string>(type: "text", nullable: true),
                    ThumbNail = table.Column<string>(type: "text", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Answers = table.Column<string[]>(type: "text[]", nullable: true),
                    CorrectIdx = table.Column<int>(type: "integer", nullable: false),
                    TQuizCollectionEId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quiz", x => x.EId);
                    table.ForeignKey(
                        name: "FK_Quiz_QuizCollection_TQuizCollectionEId",
                        column: x => x.TQuizCollectionEId,
                        principalTable: "QuizCollection",
                        principalColumn: "EId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_TQuizCollectionEId",
                table: "Quiz",
                column: "TQuizCollectionEId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizCollection_OwnerEId",
                table: "QuizCollection",
                column: "OwnerEId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quiz");

            migrationBuilder.DropTable(
                name: "QuizCollection");
        }
    }
}
