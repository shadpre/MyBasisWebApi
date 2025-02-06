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
        private readonly MyDbContext _context; // Database context instance
        private readonly IMapper _mapper; // Mapper instance for object mapping

        /// <summary>
        /// Initializes a new instance of the GenericRepository class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="mapper">The mapper instance.</param>
        public GenericRepository(MyDbContext context, IMapper mapper)
        {
            _context = context; // Assign the database context instance
            _mapper = mapper; // Assign the mapper instance
        }

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
        public async Task<T> AddAsync(T entity)
        {
            await _context.AddAsync(entity); // Add the entity to the context
            await _context.SaveChangesAsync(); // Save changes to the database
            return entity; // Return the added entity
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
            var entity = _mapper.Map<T>(source); // Map the source entity to the target entity type

            await _context.AddAsync(entity); // Add the entity to the context
            await _context.SaveChangesAsync(); // Save changes to the database

            return _mapper.Map<TResult>(entity); // Map and return the added entity to the result type
        }

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id); // Get the entity by its ID

            if (entity is null) // If entity is not found, throw NotFoundException
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            _context.Set<T>().Remove(entity); // Remove the entity from the context
            await _context.SaveChangesAsync(); // Save changes to the database
        }

        /// <summary>
        /// Checks if an entity exists by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the entity exists.</returns>
        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id); // Get the entity by its ID
            return entity != null; // Return true if entity exists, otherwise false
        }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities.</returns>
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync(); // Get and return all entities as a list
        }

        /// <summary>
        /// Gets all entities with pagination and maps them to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="queryParameters">The query parameters for pagination.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paged result of mapped results.</returns>
        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            var totalSize = await _context.Set<T>().CountAsync(); // Get total count of entities
            var items = await _context.Set<T>()
                .Skip(queryParameters.StartIndex) // Skip entities based on start index
                .Take(queryParameters.PageSize) // Take entities based on page size
                .ProjectTo<TResult>(_mapper.ConfigurationProvider) // Project entities to result type using AutoMapper
                .ToListAsync(); // Convert to list asynchronously

            return new PagedResult<TResult>
            {
                Items = items, // Set paged items
                PageNumber = queryParameters.PageNumber, // Set page number
                RecordNumber = queryParameters.PageSize, // Set record number (page size)
                TotalCount = totalSize // Set total count of entities
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
                .ProjectTo<TResult>(_mapper.ConfigurationProvider) // Project entities to result type using AutoMapper
                .ToListAsync(); // Convert to list asynchronously
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
        public async Task<T> GetAsync(int? id)
        {
            var result = await _context.Set<T>().FindAsync(id); // Find and return the entity by its ID asynchronously
            if (result is null) // If entity is not found, throw NotFoundException
            {
                throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No Key Provided");
            }

            return result; // Return found entity
        }

        /// <summary>
        /// Gets an entity by its ID and maps it to a result type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the mapped result.</returns>
        public async Task<TResult> GetAsync<TResult>(int? id)
        {
            var result = await _context.Set<T>().FindAsync(id); // Find and return the entity by its ID asynchronously
            if (result is null) // If entity is not found, throw NotFoundException
            {
                throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No Key Provided");
            }

            return _mapper.Map<TResult>(result); // Map and return the found entity to the result type
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity); // Update the entity in the context
            await _context.SaveChangesAsync(); // Save changes to the database
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
            if (id != source.Id) // If ID does not match, throw BadRequestException
            {
                throw new BadRequestException("Invalid Id used in request");
            }

            var entity = await GetAsync(id); // Get the entity by its ID

            if (entity == null) // If entity is not found, throw NotFoundException
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            _mapper.Map(source, entity); // Map the source entity to the target entity
            _context.Update(entity); // Update the entity in the context
            await _context.SaveChangesAsync(); // Save changes to the database
        }
    }
}