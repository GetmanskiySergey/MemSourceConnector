namespace MemSourceConnector.Api
{
    /// <summary>
    /// Export response description
    /// </summary>
    public class ExportTransMemoriesResponse
    {
        public AsyncRequest asyncRequest { get; set; }
        public AsyncExport asyncExport { get; set; }
    }
}
