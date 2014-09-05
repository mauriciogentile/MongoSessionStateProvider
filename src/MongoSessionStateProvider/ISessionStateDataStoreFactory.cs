using System;
using System.Collections.Specialized;

namespace AspNet.Session.MongoSessionStateProvider
{
    public interface ISessionStateDataStoreFactory : IDisposable
    {
        void Initialize(string name, NameValueCollection config);
        ISessionStateDataStore Get();
    }
}
