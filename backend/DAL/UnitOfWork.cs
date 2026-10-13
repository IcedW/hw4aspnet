using mvc.DAL.Interfaces;
using mvc.DAL.Repositories;

namespace mvc.DAL;

public class UnitOfWork : IUnitOfWork
{
    private readonly MovieContext _context;
    public IRepository<Movie> Movies { get; }

    public UnitOfWork(MovieContext context)
    {
        _context = context;
        Movies = new Repository<Movie>(context);
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
