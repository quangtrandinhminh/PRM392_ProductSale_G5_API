using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repositories.Extensions;
using Repositories.Models;

namespace Repositories.Base;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Set();
    IQueryable<T?> GetAll();
    Task<PaginatedList<T>> GetAllPaginatedQueryable(
               int pageNumber,
                      int pageSize,
                      Expression<Func<T, bool>> predicate = null,
                      params Expression<Func<T, object>>[]? includeProperties
                  );
    IQueryable<T> GetAllWithCondition(Expression<Func<T, bool>> predicate = null,
               params Expression<Func<T, object>>[] includeProperties);
    Task<IList<T>?> GetAllAsync();
    Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate);
    Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate,
               params Expression<Func<T, object>>[] includeProperties);
    void Create(T entity);
    void Update(T entity);
    bool Remove(T entity);
    Task<int> SaveChangeAsync();
    int SaveChange();
    Task<T> AddAsync(T entity);
}
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public GenericRepository()
    {
        _context ??= new AppDbContext();
    }

    private DbSet<T> _dbSet;

    protected DbSet<T> DbSet
    {
        get
        {
            if (_dbSet != null)
            {
                return _dbSet;
            }

            _dbSet = _context.Set<T>();
            return _dbSet;
        }
    }

    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<T> Set() => DbSet.AsNoTracking();

    public IQueryable<T?> GetAll()
    {
        return DbSet.AsQueryable().AsNoTracking();
    }

    // get all with paging, return paginated queryable and all page count
    // can only select the columns that are needed in the response
    public async Task<PaginatedList<T>> GetAllPaginatedQueryable(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>> predicate = null,
        params Expression<Func<T, object>>[]? includeProperties
        )
    {
        IQueryable<T> queryable = DbSet.AsNoTracking();
        includeProperties = includeProperties?.Distinct().ToArray();

        // Include related entities
        if (includeProperties?.Any() ?? false)
        {
            foreach (var navigationPropertyPath in includeProperties)
            {
                queryable = queryable.Include(navigationPropertyPath);
            }
        }

        // Apply the predicate
        queryable = predicate != null ? queryable.Where(predicate) : queryable;

        // Create the paginated list with the projected query
        return await PaginatedList<T>.CreateAsync(queryable, pageNumber, pageSize);
    }

    public IQueryable<T> GetAllWithCondition(Expression<Func<T, bool>> predicate = null,
        params Expression<Func<T, object>>[] includeProperties)
    {
        IQueryable<T> queryable = DbSet.AsNoTracking();
        includeProperties = includeProperties?.Distinct().ToArray();
        if (includeProperties?.Any() ?? false)
        {
            Expression<Func<T, object>>[] array = includeProperties;
            foreach (Expression<Func<T, object>> navigationPropertyPath in array)
            {
                queryable = queryable.Include(navigationPropertyPath);
            }

            queryable = queryable.AsSplitQuery();
        }

        return predicate != null ? queryable.Where(predicate) : queryable;
    }

    public async Task<IList<T>?> GetAllAsync()
    {
        return await DbSet.AsQueryable().AsNoTracking().ToListAsync();
    }

    // check if an entity exists
    public async Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.AnyAsync(predicate);
    }

    public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includeProperties)
    {

        var query = DbSet.AsQueryable();

        if (includeProperties != null)
        {
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            query = query.AsSplitQuery();
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    public void Create(T entity)
    {
        _context.Add(entity);
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public bool Remove(T entity)
    {
        _context.Remove(entity);
        return true;
    }

    public async Task<int> SaveChangeAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public int SaveChange()
    {
        return _context.SaveChanges();
    }

    public async Task<T> AddAsync(T entity)
    {
        _context.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}

