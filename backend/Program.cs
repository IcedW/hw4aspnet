using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using mvc.DAL;
using mvc.Localization;
using mvc.Resources;
using mvc.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MovieContext>(options => options.UseSqlServer(connection));

// Локалізація: ресурси (.resx) лежать у папці Resources поряд із SharedResource
builder.Services.AddLocalization();

builder.Services.AddAuthorization();\nbuilder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie(options =>
{
    options.Cookie.Name = "KinoPoshuk.Auth";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
})
.AddOAuth("GitHub", options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "";
    options.CallbackPath = "/signin-github";
    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.SaveTokens = true;
    options.Scope.Add("read:user");
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.ParseAdd("application/json");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
            request.Headers.UserAgent.ParseAdd("KinoPoshuk");
            using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();
            using var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
            context.RunClaimActions(user.RootElement);
        }
    };
});

builder.Services.AddControllers()
    // повідомлення валідації та назви полів ([Display]) беруться з SharedResource*.resx
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource)));
builder.Services.AddApplicationServices();

// Дозволяємо звернення з React-додатка (запускається на іншому порту у режимі розробки)
const string ReactAppCors = "ReactAppCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(ReactAppCors, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()\n              .AllowCredentials();
    });
});

// Мову запиту визначаємо за ?culture=xx, cookie або заголовком Accept-Language
var supportedCultures = AppCultures.Supported.Select(c => new CultureInfo(c)).ToList();
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(AppCultures.Default),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

var app = builder.Build();

app.UseStaticFiles();
app.UseCors(ReactAppCors);
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestLocalization(localizationOptions);
app.MapGet("/auth/github", (HttpContext context) =>
    context.ChallengeAsync("GitHub", new AuthenticationProperties
    {
        RedirectUri = builder.Configuration["Frontend:Url"] ?? "http://localhost:5173"
    }));

app.MapGet("/auth/me", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    return Results.Ok(new
    {
        isAuthenticated = true,
        username = context.User.Identity.Name,
        email = context.User.FindFirst(ClaimTypes.Email)?.Value
    });
}).RequireAuthorization();

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.MapControllers();

app.Run();
