﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Redis
{
    public class RedisHost
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6379;
    }
}
