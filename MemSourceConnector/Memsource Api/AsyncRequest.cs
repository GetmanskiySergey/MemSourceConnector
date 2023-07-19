using System;

namespace MemSourceConnector.Api
{
    /// <summary>
    /// Async request description
    /// </summary>
    public class AsyncRequest
    {
        /// <summary>
        /// Request id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Created by
        /// </summary>
        public User createdBy { get; set; }

        /// <summary>
        /// Date created
        /// </summary>
        public DateTime dateCreated { get; set; }

        /// <summary>
        /// Current state
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// Asunc request result
        /// </summary>
        public AsyncResponse asyncResponse { get; set; }

        /// <summary>
        /// Project description
        /// </summary>
        public ProjectInfo project { get; set; }
    }
}
