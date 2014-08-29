using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoSessionStateProvider
{
    public class MongoRepository
    {
        protected static MongoDatabase Db { get; private set; }
        protected static readonly object Sync = new object();

        public MongoRepository(string connectionString)
        {
            if (Db != null) return;
            lock (Sync)
            {
                if (Db != null) return;
                BsonSerializer.RegisterSerializer(typeof(ObjectId), new IdentitySerializer());
                BsonSerializer.RegisterIdGenerator(typeof(string), new IdentityGenerator());

                var server = new MongoClient(connectionString).GetServer();
                var uri = new Uri(connectionString);
                var dbName = uri.Segments[uri.Segments.Length - 1];

                Db = server.GetDatabase(dbName);
            }
        }
    }
}
