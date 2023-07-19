using Newtonsoft.Json;

namespace MemSourceConnector.Api
{
    /// <summary>
    /// Auth request parameters
    /// </summary>
    public class AuthBody
    {
        /// <summary>
        /// Username
        /// </summary>
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }
}
