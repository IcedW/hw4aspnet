namespace mvc.BLL.DTOs;

// Фільм з усіма перекладами — для форми редагування.
public class MovieEditDto
{
    public int Id { get; set; }
    public int ReleaseYear { get; set; }
    public string PosterPath { get; set; } = string.Empty;
    public List<MovieTranslationDto> Translations { get; set; } = new();
}
