using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks
{
    public enum TransactionLockMode
    {
        Read,
        Write
    }

    public enum ResultType
    {
        First,
        List,
        Count
    }

    public enum QueryParameterType
    {
        None,
        WorkOf,
        UserOf,
        PrimaryOf,
        KeyOf
    }

    public enum OperatorType
    {
        None,
        Equal,
        In
    }

    public enum TransactionState
    {
        Active,
        Committed,
        Aborted,
        Disposed
    }

    public enum ScopeType
    {
        None,
        Core,
        Engine,
        Serializer,
        Watcher,
        Reader,
        Writer,
        Query,
        Repository,
        Transaction
    }

    //important index.. use document writer..partition queue
    public enum OperationType
    {
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        Create = 1,
        Append = 2,
        Merge = 3,
        Delete = 4,
        Load = 5,
        Truncate = 6
    }

    public enum DataType
    {
        None,
        Object,
        Array,
        String,
        Markdown,
        Html,
        Json
    }

    public enum CacheType
    {
        None,
        Object,
        Array
    }

    public enum OrderByDirection
    {
        None,
        Asc,
        Desc
    }

    public enum Format
    {
        None,
        Indented
    }

    public enum MapType
    {
        None,
        /// <summary>
        /// 1:1
        /// </summary>
        OneToOne,
        /// <summary>
        /// M:1
        /// </summary>
        ManyToOne,
        /// <summary>
        /// M:M
        /// </summary>
        ManyToMany,
        /// <summary>
        /// 1:M
        /// </summary>
        OneToMany
    }
}
