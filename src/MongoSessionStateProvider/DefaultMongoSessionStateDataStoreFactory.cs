using System.Collections.Specialized;
using System.Configuration;

namespace AspNet.Session.MongoSessionStateProvider
{
    public sealed class DefaultMongoSessionStateDataStoreFactory : ISessionStateDataStoreFactory
    {
        MongoSessionStateDataStore _repository;
        #region ISessionStateDataStoreFactory Members

        public void Initialize(string name, NameValueCollection config)
        {
            string connectionStringName = config["connectionStringName"];
            if (string.IsNullOrEmpty(connectionStringName))
            {
                throw new ConfigurationErrorsException();
            }

            _repository = new MongoSessionStateDataStore(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
        }

        public ISessionStateDataStore Get()
        {
            return _repository;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _repository.Dispose();
        }

        #endregion
    }
}
