namespace MemSourceConnector.Api
{
    /// <summary>
    /// User description
    /// </summary>
    public class User
    {
        /// <summary>
        /// Login
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// Identificator
        /// </summary>
        public string uid { get; set; }

        /// <summary>
        /// Inner id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Firstname
        /// </summary>
        public string firstName { get; set; }

        /// <summary>
        /// Lastname
        /// </summary>
        public string lastName { get; set; }

        /// <summary>
        /// User role
        /// </summary>
        public string role { get; set; }

        /// <summary>
        /// User email
        /// </summary>
        public string email { get; set; }
    }
}
