namespace mvc.DAL
{
    // Переклад текстових полів фільму однією мовою ("uk", "en", "ru").
    // Пара (MovieId, Culture) унікальна.
    public class MovieTranslation
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public string Culture { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
