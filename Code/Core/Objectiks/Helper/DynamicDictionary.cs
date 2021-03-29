using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Objectiks.Helper
{
    /// <summary>
    /// dynamic x = new DynamicDictionary(new Dictionary<string, object> {{"Name", "Peter"}});
    /// Console.WriteLine(x.Name);
    /// </summary>
    public class DynamicDictionary : DynamicObject
    {
        private readonly Dictionary<string, object> dictionary;

        public DynamicDictionary(Dictionary<string, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            return dictionary.TryGetValue(binder.Name, out result);
        }

        public object GetValue(string key)
        {
            if (dictionary != null && dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return null;
        }

        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            dictionary[binder.Name] = value;

            return true;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return dictionary;
        }
    }
}
