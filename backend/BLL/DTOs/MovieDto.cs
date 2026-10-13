namespace mvc.BLL.DTOs;

// Фільм, локалізований мовою поточного запиту (Accept-Language / ?culture=).
public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string PosterPath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Мова, якою фактично повернуто текст (може відрізнятись від запитаної, якщо перекладу немає)
    public string Culture { get; set; } = string.Empty;

    // Мови, для яких у БД є переклад
    public List<string> AvailableCultures { get; set; } = new();
}
