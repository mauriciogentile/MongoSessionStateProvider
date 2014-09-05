using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AspNet.Session.MongoSessionStateProvider
{
    public class SessionStateData
    {
        public SessionStateData()
        {
            Created = DateTime.UtcNow;
            LockDate = DateTime.UtcNow;
        }

        [BsonId]
        public string Id { get; set; }
        public string SessionId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public bool Initialized { get; set; }
        public bool Locked { get; set; }
        public DateTime LockDate { get; set; }
        public int LockCookie { get; set; }
        public int Timeout { get; set; }
        public long ItemSize { get; set; }
        public byte[] Payload { get; set; }
    }
}
