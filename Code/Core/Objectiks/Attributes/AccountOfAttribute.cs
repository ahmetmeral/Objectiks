﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AccountOfAttribute : Attribute
    {
        public AccountOfAttribute() { }
    }
}
