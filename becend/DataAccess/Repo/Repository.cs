using DataAccess.Data;
using DataAccess.Helpers;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repo
{
    public class Repository<T> : IRepository<T> where T : class, BaseEntity
    {
        internal QuizRushContext context;
        internal DbSet<T> set;

        public Repository(QuizRushContext context)
        {
            this.context = context;
            this.set = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await set.AddAsync(entity);
            await this.context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            await DeleteAsync(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity != null)
            {
                set.Remove(entity);
                await context.SaveChangesAsync();
            }
        }
        public async Task ClearAsync()
        {
            var set = context.Set<T>();
            if (set.Any())
            {
                context.RemoveRange(set);
                await context.SaveChangesAsync();
            }
        }


        public async Task<IReadOnlyList<T>> GetAllAsync(int? pageNumber = 1, int pageSize = 5, Expression<Func<T, bool>>? filtering = null, params string[]? includes)
        {
            var query = set.AsQueryable();

            if (pageNumber != null)
                query = await query.PaginateAsync(pageNumber.Value, pageSize);

            if (filtering != null)
                query = query.Where(filtering);

            if (includes != null && includes.Length > 0)
                foreach (var prop in includes)
                    query = query.Include(prop);

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await set.FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    }
}
