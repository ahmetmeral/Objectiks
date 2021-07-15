using Objectiks.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Objectiks.Redis
{
    public class RedisDatabase
    {
        public int Number { get; private set; }
        public IDocumentSerializer Serializer { get; private set; }
        public RedisConnection Connection { get; private set; }

        internal IDatabase Database
        {
            get
            {
                return Connection.GetConnection().GetDatabase(Number);
            }
        }

        public RedisDatabase(RedisConnection connection, IDocumentSerializer serializer, int databaseNumber)
        {
            Connection = connection;
            Serializer = serializer;
            Number = databaseNumber;
        }

        public bool Set(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Database.StringSet(key, value, null, when, flag);
        }

        public Task<bool> SetAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Database.StringSetAsync(key, value, null, when, flag);
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


        public bool Set(RedisKey key, byte[] bytes, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Database.StringSet(key, bytes, null, when, flag);
        }

        public Task<bool> SetAsync(RedisKey key, byte[] bytes, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Database.StringSetAsync(key, bytes, null, when, flag);
        }


        public bool Set(RedisKey key, RedisValue value, int expiry, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Database.StringSet(key, value, TimeSpan.FromMinutes(expiry), when, flag);
        }

        public Task<bool> SetAsync(RedisKey key, RedisValue value, int expiry, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Database.StringSetAsync(key, value, TimeSpan.FromMinutes(expiry), when, flag);
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


        public bool Set(RedisKey key, byte[] bytes, int expiry, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Database.StringSet(key, bytes, TimeSpan.FromMinutes(expiry), when, flag);
        }

        public Task<bool> SetAsync(RedisKey key, byte[] bytes, int expiry, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
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

        public byte[] Get(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var bytes = Database.StringGet(key, flags);

            if (!bytes.HasValue)
            {
                return default;
            }

            return bytes;
        }

        public async Task<byte[]> GetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var bytes = await Database.StringGetAsync(key, flags).ConfigureAwait(false);

            if (!bytes.HasValue)
            {
                return default;
            }

            return bytes;
        }


        public bool Remove(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDelete(key);
        }

        public Task<bool> RemoveAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDeleteAsync(key, flags);
        }

        public bool Exists(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyExists(key, flags);
        }

        public Task<bool> ExistsAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyExistsAsync(key, flags);
        }

        public void Flush()
        {
            var endPoints = Database.Multiplexer.GetEndPoints();

            var tasks = new List<Task>(endPoints.Length);

            for (var i = 0; i < endPoints.Length; i++)
            {
                var server = Database.Multiplexer.GetServer(endPoints[i]);

                if (!server.IsReplica)
                    tasks.Add(server.FlushDatabaseAsync(Number));
            }

            Task.WhenAll(tasks);
        }
    }
}
