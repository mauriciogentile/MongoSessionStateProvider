using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoSessionStateProvider
{
    public class MongoSessionStateDataStore : MongoRepository, ISessionStateDataStore
    {
        readonly MongoCollection<SessionStateData> _collection;

        #region ISessionStateDataStore Members

        public MongoSessionStateDataStore(string connectionString)
            : base(connectionString)
        {
            _collection = Db.GetCollection<SessionStateData>(typeof(SessionStateData).Name);
        }

        public SessionStateData Get(string id)
        {
            return _collection.FindOneById(BsonValue.Create(new ObjectId(id)));
        }

        public void Save(SessionStateData data)
        {
            _collection.Save(data);
        }

        public void Delete(string id)
        {
            _collection.Remove(new QueryDocument("_id", new ObjectId(id)));
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
