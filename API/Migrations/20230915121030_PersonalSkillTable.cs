using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HallOfFame.Migrations
{
    /// <inheritdoc />
    public partial class PersonalSkillTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Persons_PersonId",
                table: "Skills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Skills",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_PersonId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Skills");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Skills",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Skills",
                table: "Skills",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PersonalSkills",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<long>(type: "bigint", nullable: false),
                    SkillId = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalSkills_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalSkills_PersonId",
                table: "PersonalSkills",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalSkills_SkillId",
                table: "PersonalSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Skills",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Skills");

            migrationBuilder.AddColumn<byte>(
                name: "Level",
                table: "Skills",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<long>(
                name: "PersonId",
                table: "Skills",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Skills",
                table: "Skills",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_PersonId",
                table: "Skills",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Persons_PersonId",
                table: "Skills",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id");
        }
    }
}
