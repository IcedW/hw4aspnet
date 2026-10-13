using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using mvc.DAL.Interfaces;

namespace mvc.DAL.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly MovieContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(MovieContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();
    // FirstOrDefaultAsync (а не FindAsync), щоб працював AutoInclude для перекладів
    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);
    public void Add(T entity) => _dbSet.Add(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
}
