using Objectiks.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Objectiks.Extentions
{
    public static class TypeExtentions
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static bool IsDynamic(this Type objectType)
        {
            return objectType.FullName == "System.Object";
        }

        public static bool IsNullable(this PropertyInfo property)
        {
            if (property.PropertyType.IsGenericType
               && property.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                //Nullable.GetUnderlyingType(property.PropertyType)
                return true;
            }
            return false;
        }

        public static Type GetNullableUnderlyingType(this PropertyInfo property)
        {
            return Nullable.GetUnderlyingType(property.PropertyType);
        }

        public static PropertyInfo[] FindProperties(this Type objectType)
        {
            if (Properties.ContainsKey(objectType))
            {
                return Properties[objectType];
            }

            BindingFlags flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo[] propertyItems = objectType.GetProperties(flag);

            Properties.TryAdd(objectType, propertyItems);

            return propertyItems;
        }




        public static IOrderedEnumerable<TSource> OrderByWithDirection<TSource, TKey>
    (this IEnumerable<TSource> source,
     Func<TSource, TKey> keySelector,
     bool descending)
        {
            return descending ? source.OrderByDescending(keySelector)
                              : source.OrderBy(keySelector);
        }

        public static IOrderedQueryable<TSource> OrderByWithDirection<TSource, TKey>
            (this IQueryable<TSource> source,
             Expression<Func<TSource, TKey>> keySelector,
             bool descending)
        {
            return descending ? source.OrderByDescending(keySelector)
                              : source.OrderBy(keySelector);
        }

        public static T ChangeType<T>(this object obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static T GetCustomAttribute<T>(this Type typ) where T : Attribute
        {
            //GetCustomAttributes  false du true ya çektik..
            object[] objArray = typ.GetCustomAttributes(typeof(T), false);
            if (objArray != null && objArray.Length > 0)
            {
                return (T)objArray[0];
            }
            return (T)null;
        }

        public static T GetCustomAttribute<T>(this Type typ, bool inherit) where T : Attribute
        {
            //GetCustomAttributes  false du true ya çektik..
            object[] objArray = typ.GetCustomAttributes(typeof(T), inherit);
            if (objArray != null && objArray.Length > 0)
            {
                return (T)objArray[0];
            }
            return (T)null;
        }

        public static T GetAttribute<T>(this PropertyInfo pi) where T : Attribute
        {
            object[] attributes = pi.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 0)
                return null;
            return attributes[0] as T;
        }

        public static T[] GetAttributes<T>(this PropertyInfo pi) where T : Attribute
        {
            object[] attributes = pi.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 0)
                return null;
            return attributes as T[];
        }

        public static object GetPropValue(this object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }

        public static void SetPropValue(this object obj, string propName, object value)
        {
            obj.GetType().GetProperty(propName).SetValue(obj, value);
        }

        public static bool IsGenericType(this Type type)
        {
#if NESTANDARD13
        
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NESTANDARD13
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if NESTANDARD13
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }


    }
}
