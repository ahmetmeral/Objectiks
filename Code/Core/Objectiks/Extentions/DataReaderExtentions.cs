using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Objectiks.Extentions
{
    public static class DataReaderExtentions
    {
        public static IEnumerable<IDataRecord> ToDataRecords(this DbDataReader reader)
        {
            foreach (IDataRecord record in reader as IEnumerable)
                yield return record; //yield return to keep the reader open
        }

        public static IEnumerable<Dictionary<string, object>> ToDictionary(this DbDataReader reader)
        {
            var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            foreach (IDataRecord record in reader as IEnumerable)
                yield return names.ToDictionary(n => n, n => record[n]);
        }

        public static IEnumerable<dynamic> ToList(this DbDataReader reader)
        {
            var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            foreach (IDataRecord record in reader as IEnumerable)
            {
                var expando = new ExpandoObject() as IDictionary<string, object>;
                foreach (var name in names)
                    expando[name] = record[name];

                yield return expando;
            }
        }

        public static IEnumerable<JObject> ToObjectList(this DbDataReader reader)
        {
            var names = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            foreach (IDataRecord record in reader as IEnumerable)
            {
                var expando = new JObject();
                foreach (var name in names)
                    expando[name] = record[name].ToString();

                yield return expando;
            }
        }
    }
}
