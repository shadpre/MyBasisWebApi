using BLL.Models;

namespace BLL.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
        Task<T> GetAsync(int? id);

        /// <summary>
        /// Gets an entity by its ID and maps it to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the mapped result.</returns>
        Task<TResult> GetAsync<TResult>(int? id);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities.</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Gets all entities and maps them to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of mapped results.</returns>
        Task<List<TResult>> GetAllAsync<TResult>();

        /// <summary>
        /// Gets all entities with pagination and maps them to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="queryParameters">The query parameters for pagination.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paged result of mapped results.</returns>
        Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Adds a new entity and maps it to a result type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the mapped result.</returns>
        Task<TResult> AddAsync<TSource, TResult>(TSource source);

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Updates an existing entity by its ID and maps it to a source type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="source">The source entity.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateAsync<TSource>(int id, TSource source) where TSource : IBaseDto;

        /// <summary>
        /// Checks if an entity exists by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the entity exists.</returns>
        Task<bool> Exists(int id);
    }
}