using System;
using System.Collections.Generic;
using System.Text;

namespace PollyFactory.ConfigSections
{
    public class PollyConfigEntry
    {
        /// <summary>
        /// Policy type
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Delays between attempts in seconds
        /// </summary>
        public List<int> Waits { get; set; }
        
        /// <summary>
        /// Number of attempts
        /// </summary>
        public int Retries { get; set; } = 1;
        
        /// <summary>
        /// Execution timeout
        /// </summary>
        public int Timeout { get; set; }
    }
}
