using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Redis
{
    public class RedisConnectionInformation
    {
        /// <summary>
        /// Gets or sets the connection pool desiderated size.
        /// </summary>
        public int PoolSize { get; set; }

        /// <summary>
        /// Gets or sets the number of active connections in the connection pool.
        /// </summary>
        public int Active { get; set; }

        /// <summary>
        /// Gets or sets the number of invalid connections in the connection pool.
        /// </summary>
        public int Invalid { get; set; }
    }
}
