using System.Collections.Generic;

namespace MemSourceConnector.Api
{
    /// <summary>
    /// Export transmem request parameters
    /// </summary>
    public class ExportTransMemoriesBody
    {
        /// <summary>
        /// not use
        /// </summary>
        public string callbackUrl { get; set; }

        /// <summary>
        /// exported languages
        /// </summary>
        public IEnumerable<string> exportTargetLangs { get; set; }
    }
}
