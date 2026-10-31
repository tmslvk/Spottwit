using Spottwit.Server.Model;
using Spottwit.Server.Models.Entities;
using Spottwit.Server.Repositories;

namespace Spottwit.Server.Services
{
    public class GenericService<T> : IGenericService<T> where T : BaseEntity
    {
        protected readonly IRepository<T> _repository;
        protected readonly ILogger<GenericService<T>> _logger;

        public GenericService(IRepository<T> repository, ILogger<GenericService<T>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            _logger.LogInformation("Запрос всех записей для сущности {EntityType}", typeof(T).Name);
            return await _repository.GetAllAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Запрос записи {Id} для сущности {EntityType}", id, typeof(T).Name);
            return await _repository.GetByIdAsync(id);
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            _logger.LogInformation("Создание новой записи для сущности {EntityType}", typeof(T).Name);
            return await _repository.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _logger.LogInformation("Обновление записи {Id} для сущности {EntityType}", entity.Id, typeof(T).Name);
            await _repository.UpdateAsync(entity);
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("Удаление записи {Id} для сущности {EntityType}", id, typeof(T).Name);
            await _repository.DeleteAsync(id);
        }
    }
}