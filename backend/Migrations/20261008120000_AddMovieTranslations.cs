using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hw1aspnet.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Director = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieTranslations_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieTranslations_MovieId_Culture",
                table: "MovieTranslations",
                columns: new[] { "MovieId", "Culture" },
                unique: true);

            // Переносимо наявні дані як українські переклади ("uk" — мова за замовчуванням)
            migrationBuilder.Sql(@"
                INSERT INTO MovieTranslations (MovieId, Culture, Title, Director, Genre, Description)
                SELECT Id, N'uk', Title, Director, Genre, Description FROM Movies");

            migrationBuilder.DropColumn(name: "Title", table: "Movies");
            migrationBuilder.DropColumn(name: "Director", table: "Movies");
            migrationBuilder.DropColumn(name: "Genre", table: "Movies");
            migrationBuilder.DropColumn(name: "Description", table: "Movies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title", table: "Movies", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(
                name: "Director", table: "Movies", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(
                name: "Genre", table: "Movies", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(
                name: "Description", table: "Movies", type: "nvarchar(max)", nullable: false, defaultValue: "");

            // Повертаємо дані: беремо український переклад, а якщо його немає — найстаріший наявний
            migrationBuilder.Sql(@"
                UPDATE m
                SET m.Title = t.Title, m.Director = t.Director, m.Genre = t.Genre, m.Description = t.Description
                FROM Movies m
                CROSS APPLY (
                    SELECT TOP 1 Title, Director, Genre, Description
                    FROM MovieTranslations
                    WHERE MovieId = m.Id
                    ORDER BY CASE WHEN Culture = N'uk' THEN 0 ELSE 1 END, Id
                ) t");

            migrationBuilder.DropTable(name: "MovieTranslations");
        }
    }
}
