﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Models
{
    public class DocumentSchemaKeys : List<string> { }

    public class DocumentSchema : IDisposable
    {
        /// <summary>
        /// Sabit alan cache için kullanılacak
        /// </summary>
        public string TypeOf { get; set; }
        /// <summary>
        /// Sabit alan cache için kullanılacak
        /// </summary>
        public string ParseOf { get; set; }
        /// <summary>
        /// Identity Property
        /// </summary>
        public string Primary { get; set; }
        /// <summary>
        /// Unique Keys..
        /// </summary>
        public DocumentSchemaKeys KeyOf { get; set; }
        /// <summary>
        /// Nesnenin ilişkilerini saklar
        /// </summary>
        public DocumentRefs Refs { get; set; }
        /// <summary>
        /// Nesnenin cache de nasıl saklanacağını belirler..
        /// Çok sık erişilen nesneleri JObject olarak saklamak daha mantıklı gibi.
        /// </summary>
        public DocumentCacheInfo Cache { get; set; }
        /// <summary>
        /// custom variables..
        /// </summary>
        public DocumentVars Vars { get; set; }

        public static DocumentSchema Default()
        {
            return new DocumentSchema();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}