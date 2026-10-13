# Кінопошук — WebApi + React

Проєкт переведено з ASP.NET Core MVC на **ASP.NET Core WebApi**: контролер `MoviesController`
більше не повертає HTML-сторінки — усі методи (`GET`, `GET /{id}`, `POST`, `PUT`, `DELETE`)
повертають JSON. Доступ до даних, як і раніше, здійснюється лише через шари
`Service → IRepository/UnitOfWork → DbContext`, контролер напряму з `DbContext` не працює.

## Структура репозиторію

```
backend/   — ASP.NET Core WebApi (.NET 10, EF Core, SQL Server)
frontend/  — React (Vite) додаток, який звертається до API
```

## Запуск бекенду (WebApi)

```bash
cd backend
dotnet restore
dotnet ef database update   # застосувати міграції (потрібен dotnet-ef)
dotnet run
```

API буде доступне на `http://localhost:5253/api/movies` (порт — з `Properties/launchSettings.json`).

Ендпоінти:

| Метод  | Маршрут              | Опис                                   |
|--------|-----------------------|-----------------------------------------|
| GET    | /api/movies            | усі фільми                              |
| GET    | /api/movies/{id}        | один фільм за id                        |
| GET    | /api/movies/{id}/translations | фільм з усіма перекладами (для форми редагування) |
| POST   | /api/movies            | створити фільм (multipart/form-data)    |
| PUT    | /api/movies/{id}        | оновити фільм (multipart/form-data)     |
| DELETE | /api/movies/{id}        | видалити фільм                          |

## Запуск фронтенду (React)

```bash
cd frontend
npm install
npm run dev
```

Додаток відкриється на `http://localhost:5173` і звертатиметься до API за адресою,
вказаною у `frontend/.env` (`VITE_API_URL`, за замовчуванням `http://localhost:5253/api`).

CORS на бекенді дозволяє звернення саме з `http://localhost:5173` (налаштування —
у `backend/appsettings.json`, секція `Cors:AllowedOrigins`).

## Багатомовність (uk / en / ru)

Мова за замовчуванням — українська (`uk`). Підтримуються `uk`, `en`, `ru`.

**БД.** Мовонезалежні дані (рік, постер) лежать у `Movies`, а текстові поля (назва, режисер, жанр, опис) —
у таблиці `MovieTranslations` (`MovieId`, `Culture`, ...), пара `(MovieId, Culture)` унікальна.
Міграція `AddMovieTranslations` переносить наявні фільми в переклади з `Culture = 'uk'`.
Якщо перекладу запитаною мовою немає, повертається `uk`, а потім будь-який наявний.

**Бекенд.** Мову запиту визначає `RequestLocalizationMiddleware` (`?culture=xx`, cookie або заголовок
`Accept-Language`). Рядки — у `Resources/SharedResource.resx` (uk), `SharedResource.en.resx`, `SharedResource.ru.resx`.
З них локалізуються назви полів (`[Display]`), повідомлення валідації (`DataAnnotations`, у т.ч. `NoDigits`) та
помилки сервісу/контролера. Нову мову додають у `Localization/AppCultures.cs` і додатковим `.resx`.

**Фронтенд.** `src/i18n/` — словники (`locales/*.js`), `translate()` та `I18nProvider`. Мова обирається в шапці
(`LanguageSwitcher`), зберігається в `localStorage` і передається в API заголовком `Accept-Language`.
Форма фільму має вкладки за мовами: заповнюйте одну або кілька.

Після оновлення виконайте `dotnet ef database update`.
