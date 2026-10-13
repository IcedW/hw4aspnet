namespace mvc.DAL.Interfaces;

public interface IUnitOfWork
{
    IRepository<Movie> Movies { get; }
    Task<int> SaveChangesAsync();
}
