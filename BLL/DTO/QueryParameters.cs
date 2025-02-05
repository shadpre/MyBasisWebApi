namespace BLL.Models
{
    /// <summary>
    /// Represents query parameters for pagination.
    /// </summary>
    public class QueryParameters
    {
        private int _pageSize = 15;

        /// <summary>
        /// Gets or sets the start index for the query.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the page size. Default value is 15.
        /// </summary>
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }
    }
}