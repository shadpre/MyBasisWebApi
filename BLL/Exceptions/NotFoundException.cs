namespace MyBasisWebApi.Logic.Exceptions
{
    /// <summary>
    /// Represents an exception for not found entities.
    /// Inherits from ApplicationException.
    /// </summary>
    public class NotFoundException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the NotFoundException class with a specified error message.
        /// </summary>
        /// <param name="name">The name of the entity that was not found.</param>
        /// <param name="key">The key of the entity that was not found.</param>
        public NotFoundException(string name, object key) : base($"{name} with id ({key}) was not found")
        {

        }
    }
}