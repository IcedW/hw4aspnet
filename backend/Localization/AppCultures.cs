namespace mvc.Localization;

/// <summary>
/// Мови, які підтримує застосунок. Перша (Default) використовується як
/// запасна, якщо запитаного перекладу фільму в БД немає.
/// </summary>
public static class AppCultures
{
    public const string Default = "uk";

    public static readonly string[] Supported = { "uk", "en", "ru" };

    public static bool IsSupported(string? culture) =>
        !string.IsNullOrWhiteSpace(culture) &&
        Supported.Contains(culture, StringComparer.OrdinalIgnoreCase);
}
