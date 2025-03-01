using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;

namespace Repositories.Base
{
    public class UnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private bool disposed = false;

        public UnitOfWork(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        }

        #region Save
        public int SaveChange() => _dbContext.SaveChanges();

        public Task<int> SaveChangeAsync() => _dbContext.SaveChangesAsync();
        #endregion Save

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose
    }
}