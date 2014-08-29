using System.Collections.Specialized;
using System.Configuration;

namespace MongoSessionStateProvider
{
    public sealed class DefaultMongoSessionStateDataStoreFactory : ISessionStateDataStoreFactory
    {
        MongoSessionStateDataStore _repository;
        #region ISessionStateDataStoreFactory Members

        public void Initialize(string name, NameValueCollection config)
        {
            string connectionString = config["connectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ConfigurationErrorsException();
            }

            _repository = new MongoSessionStateDataStore(connectionString);
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
