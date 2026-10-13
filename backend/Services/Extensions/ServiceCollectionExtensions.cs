using mvc.BLL.Interfaces;
using mvc.BLL.Services;
using mvc.DAL.Interfaces;
using mvc.DAL;
using mvc.Services.Implementations;
using mvc.Services.Interfaces;

namespace mvc.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IFileService, FileService>();
        return services;
    }
}
