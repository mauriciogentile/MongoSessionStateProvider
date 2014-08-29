using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoSessionStateProvider
{
    public class IdentityGenerator : IIdGenerator
    {
        static readonly IdentityGenerator instance = new IdentityGenerator();

        public static IdentityGenerator Instance
        {
            get
            {
                return instance;
            }
        }

        public object GenerateId(object container, object document)
        {
            return ObjectId.GenerateNewId().ToString();
        }

        public bool IsEmpty(object id)
        {
            return id == null || string.IsNullOrEmpty((string)id);
        }
    }
}
