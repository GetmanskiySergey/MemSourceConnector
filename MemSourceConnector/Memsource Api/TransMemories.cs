using System.Collections.Generic;

namespace MemSourceConnector.Api
{
    /// <summary>
    /// TransMemories request info
    /// </summary>
    public class TransMemories
    {
        /// <summary>
        /// Total elements in list
        /// </summary>
        public int totalElements { get; set; }

        /// <summary>
        /// Total pages
        /// </summary>
        public int totalPages { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int pageSize { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int pageNumber { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public int numberOfElements { get; set; }

        /// <summary>
        /// List of transmem DBs
        /// </summary>
        public IEnumerable<TM_Content> content { get; set; }
    }
}
