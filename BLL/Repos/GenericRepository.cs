using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Exceptions;
using BLL.Interfaces;
using BLL.Models;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace BLL.Repos
{
    /// <summary>
    /// Generic repository for data access operations.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly MyDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the GenericRepository class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="mapper">The mapper instance.</param>
        public GenericRepository(MyDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
        public async Task<T> AddAsync(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Adds a new entity and maps it to a result type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the mapped result.</returns>
        public async Task<TResult> AddAsync<TSource, TResult>(TSource source)
        {
            var entity = _mapper.Map<T>(source);

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<TResult>(entity);
        }

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);

            if (entity is null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if an entity exists by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the entity exists.</returns>
        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id);
            return entity != null;
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities.</returns>
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// Gets all entities with pagination and maps them to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="queryParameters">The query parameters for pagination.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paged result of mapped results.</returns>
        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            var totalSize = await _context.Set<T>().CountAsync();
            var items = await _context.Set<T>()
                .Skip(queryParameters.StartIndex)
                .Take(queryParameters.PageSize)
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResult<TResult>
            {
                Items = items,
                PageNumber = queryParameters.PageNumber,
                RecordNumber = queryParameters.PageSize,
                TotalCount = totalSize
            };
        }

        /// <summary>
        /// Gets all entities and maps them to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of mapped results.</returns>
        public async Task<List<TResult>> GetAllAsync<TResult>()
        {
            return await _context.Set<T>()
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
        public async Task<T> GetAsync(int? id)
        {
            var result = await _context.Set<T>().FindAsync(id);
            if (result is null)
            {
                throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No Key Provided");
            }

            return result;
        }

        /// <summary>
        /// Gets an entity by its ID and maps it to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the mapped result.</returns>
        public async Task<TResult> GetAsync<TResult>(int? id)
        {
            var result = await _context.Set<T>().FindAsync(id);
            if (result is null)
            {
                throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No Key Provided");
            }

            return _mapper.Map<TResult>(result);
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing entity by its ID and maps it to a source type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source entity.</typeparam>
        /// <param name="id">The ID of the entity to update.</param>
        /// <param name="source">The source entity.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync<TSource>(int id, TSource source) where TSource : IBaseDto
        {
            if (id != source.Id)
            {
                throw new BadRequestException("Invalid Id used in request");
            }

            var entity = await GetAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            _mapper.Map(source, entity);
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}