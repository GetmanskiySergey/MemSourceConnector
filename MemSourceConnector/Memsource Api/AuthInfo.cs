using System;

namespace MemSourceConnector.Api
{
    /// <summary>
    /// Auth response description
    /// </summary>
    public class AuthInfo
    {
        /// <summary>
        /// User info
        /// </summary>
        public User user { get; set; }

        /// <summary>
        /// Session token
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// Token expires
        /// </summary>
        public DateTime expires { get; set; }
    }
}
