using System;
using System.Collections.Generic;

namespace MemSourceConnector.Api
{
    /// <summary>
    /// Transmem description
    /// </summary>
    public class TM_Content
    {
        /// <summary>
        /// DB identificator
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Db inner identificator
        /// </summary>
        public int internalId { get; set; }

        /// <summary>
        /// DB name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// DB source language
        /// </summary>
        public string sourceLang { get; set; }

        /// <summary>
        /// DB target languages
        /// </summary>
        public IEnumerable<string> targetLangs { get; set; }
        //public IEnumerable<string> client { get; set; }
        //public IEnumerable<string> businessUnit { get; set; }
        //public IEnumerable<string> domain { get; set; }
        //public IEnumerable<string> subDomain { get; set; }

        /// <summary>
        /// DB description
        /// </summary>
        public string note { get; set; }

        /// <summary>
        /// DB created date
        /// </summary>
        public DateTime dateCreated { get; set; }

        /// <summary>
        /// DB created by user
        /// </summary>
        public User createdBy { get; set; }
    }
}
