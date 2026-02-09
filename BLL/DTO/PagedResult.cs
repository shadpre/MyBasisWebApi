namespace MyBasisWebApi.Logic.Models
{
    /// <summary>
    /// Represents a paged result set.
    /// </summary>
    /// <typeparam name="T">The type of items in the result set.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the total count of items.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of records per page.
        /// </summary>
        public int RecordNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of items.
        /// </summary>
        public List<T> Items { get; set; }
    }
}