namespace mvc.DAL
{
    // Мовонезалежні дані фільму. Текстові поля (назва, режисер, жанр, опис)
    // зберігаються окремо для кожної мови у MovieTranslation.
    public class Movie
    {
        public int Id { get; set; }

        public int ReleaseYear { get; set; }

        public string PosterPath { get; set; } = string.Empty;

        public List<MovieTranslation> Translations { get; set; } = new();
    }
}
