using System;

namespace AspNet.Session.MongoSessionStateProvider
{
    public interface ISessionStateDataStore : IDisposable
    {
        SessionStateData Get(string id);
        void Save(SessionStateData data);
        void Delete(string id);
    }
}
