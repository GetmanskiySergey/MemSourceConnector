using System;

namespace MemSourceConnector.Api
{
    /// <summary>
    /// Async request description
    /// </summary>
    public class AsyncResponse
    {       
//"errorDetails": [
//{
//"code": "string",
//"args": {
//"property1": { },
//"property2": { }
//},
//"message": "string"
//}
//],
//"warnings": [
//{
//"code": "string",
//"args": {},
//"message": "string"
//}
//]
        /// <summary>
        /// Date created
        /// </summary>
        public DateTime dateCreated { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public string errorCode { get; set; }

        /// <summary>
        /// Error description
        /// </summary>
        public string errorDesc { get; set; }

      
    }
}
