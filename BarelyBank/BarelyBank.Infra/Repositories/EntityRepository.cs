using BarelyBank.Domain.Repositories;
using BarelyBank.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace BarelyBank.Infra.Repositories
{
    public abstract class EntityRepository<T> : IEntityRepository<T> where T : class
    {
        protected readonly BBContext Context;

        protected EntityRepository(BBContext context)
        {
            Context = context; // Context.Set<T>() para acessar o DbSet da entidade T
        }

        public void Add(T entity)
        {
            Context.Set<T>().Add(entity);
        }

        public void Update(T entity)
        {
            Context.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            Context.Set<T>().Remove(entity);
        }

        public async Task<T?> GetAsync(Guid id)
        {
            return await Context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Context.Set<T>().ToListAsync();
        }

    }

}
