using LMS.Server.Core.Interfaces;
using LMS.Server.Core.Shared;
using LMS.Server.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Server.Infrastructure.Repositories
{
    public abstract class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _appDbContext;

        protected EfRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public virtual async Task<T> GetById(int id)
        {
            return await _appDbContext.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> ListAll()
        {
            return await _appDbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetSingleBySpec(ISpecification<T> spec)
        {
            var result = await ListBySpec(spec);
            return result.FirstOrDefault();
        }

        public async Task<List<T>> ListBySpec(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<T> Add(T entity)
        {
            _appDbContext.Set<T>().Add(entity);
            await _appDbContext.SaveChangesAsync();
            return entity;
        }

        public async Task Update(T entity)
        {
            _appDbContext.Entry(entity).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            _appDbContext.Set<T>().Remove(entity);
            await _appDbContext.SaveChangesAsync();
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            var query = _appDbContext.Set<T>().AsQueryable();

            // modify the IQueryable using the specification's criteria expression
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            // Includes all expression-based includes
            query = spec.Includes.Aggregate(query,
                                    (current, include) => current.Include(include));

            // Include any string-based include statements
            query = spec.IncludeStrings.Aggregate(query,
                                    (current, include) => current.Include(include));

            // Apply ordering if expressions are set
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            if (spec.GroupBy != null)
            {
                query = query.GroupBy(spec.GroupBy).SelectMany(x => x);
            }

            // Apply paging if enabled
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip)
                             .Take(spec.Take);
            }
            return query;
        }
    }
}