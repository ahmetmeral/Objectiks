using Objectiks.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Objectiks.StackExchange.Redis
{
    public class RedisDatabase
    {
        public int DatabaseNumber { get; private set; }
        public IDocumentSerializer Serializer { get; private set; }
        public RedisConnection Connection { get; private set; }

        public IDatabase Database
        {
            get
            {
                return Connection.GetConnection().GetDatabase(DatabaseNumber);
            }
        }

        public RedisDatabase(RedisConnection connection, IDocumentSerializer serializer, int databaseNumber)
        {
            Connection = connection;
            Serializer = serializer;
            DatabaseNumber = databaseNumber;
        }

        public bool Set<T>(RedisKey key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var bytes = Serializer.Serialize(value);

            return Database.StringSet(key, bytes, null, when, flag);
        }

        public Task<bool> SetAsync<T>(RedisKey key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var bytes = Serializer.Serialize(value);

            return Database.StringSetAsync(key, bytes, null, when, flag);
        }

        public bool Set<T>(RedisKey key, T value, int expiry, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var bytes = Serializer.Serialize(value);

            return Database.StringSet(key, bytes, TimeSpan.FromMinutes(expiry), when, flag);
        }

        public Task<bool> SetAsync<T>(RedisKey key, T value, int expiry, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var bytes = Serializer.Serialize(value);

            return Database.StringSetAsync(key, bytes, TimeSpan.FromMinutes(expiry), when, flag);
        }

        public T Get<T>(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var bytes = Database.StringGet(key, flags);

            if (!bytes.HasValue)
            {
                return default;
            }

            return Serializer.Deserialize<T>(bytes);
        }

        public async Task<T> GetAsync<T>(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var bytes = await Database.StringGetAsync(key, flags).ConfigureAwait(false);

            if (!bytes.HasValue)
            {
                return default;
            }

            return Serializer.Deserialize<T>(bytes);
        }

        public bool Remove(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDelete(key);
        }

        public Task<bool> RemoveAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDeleteAsync(key, flags);
        }
    }
}
