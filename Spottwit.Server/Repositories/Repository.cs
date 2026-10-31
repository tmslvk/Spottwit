using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Spottwit.Server.Data;
using Spottwit.Server.Model;
using Spottwit.Server.Models.Entities;
using System.Linq.Expressions;
using System.Text.Json;

namespace Spottwit.Server.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<Repository<T>> _logger;
        protected readonly IDistributedCache _cache;

        private readonly string _cacheKey = $"cache_{typeof(T).Name}_all";

        public Repository(
            ApplicationDbContext context,
            ILogger<Repository<T>> logger,
            IDistributedCache cache)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var cachedData = await _cache.GetStringAsync(_cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Data for {EntityType} received from Redis cache", typeof(T).Name);
                return JsonSerializer.Deserialize<IEnumerable<T>>(cachedData) ?? Enumerable.Empty<T>();
            }

            _logger.LogInformation("Request data for {EntityType} from PostgreSQL", typeof(T).Name);
            var entities = await _dbSet.ToListAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(_cacheKey, JsonSerializer.Serialize(entities), cacheOptions);

            return entities;
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Search {EntityType} with ID {Id} in DB", typeof(T).Name, id);
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.CreatedOn = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New {EntityType} record created  (ID: {Id}). Clearing cache...", typeof(T).Name, entity.Id);
            await InvalidateCacheAsync();

            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            entity.ModifiedOn = DateTime.UtcNow;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Record {EntityType} updating (ID: {Id}). Clearing cache...", typeof(T).Name, entity.Id);
            await InvalidateCacheAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Удалена запись {EntityType} (ID: {Id}). Очищаем кэш.", typeof(T).Name, id);
                await InvalidateCacheAsync();
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        private async Task InvalidateCacheAsync()
        {
            await _cache.RemoveAsync(_cacheKey);
        }
    }
}