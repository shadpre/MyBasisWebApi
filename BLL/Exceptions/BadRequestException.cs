namespace MyBasisWebApi.Logic.Exceptions
{
    /// <summary>
    /// Represents an exception for bad requests.
    /// Inherits from ApplicationException.
    /// </summary>
    public class BadRequestException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the BadRequestException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BadRequestException(string message) : base(message)
        {

        }
    }
}