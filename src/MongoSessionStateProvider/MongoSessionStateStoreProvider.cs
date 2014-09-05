using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.SessionState;

namespace AspNet.Session.MongoSessionStateProvider
{
    public class MongoSessionStateStoreProvider : SessionStateStoreProviderBase
    {
        readonly ISessionStateDataStoreFactory _storeFactory;
        private static readonly ConcurrentDictionary<string, Lock> Locks = new ConcurrentDictionary<string, Lock>();

        public MongoSessionStateStoreProvider()
            : this(new DefaultMongoSessionStateDataStoreFactory())
        {
        }

        public MongoSessionStateStoreProvider(ISessionStateDataStoreFactory sessionStateDataStoreFactory)
        {
            _storeFactory = sessionStateDataStoreFactory;
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            _storeFactory.Initialize(name, config);
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            HttpStaticObjectsCollection staticObjects = null;

            if (context != null)
            {
                staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
            }

            return new SessionStateStoreData(new SessionStateItemCollection(), staticObjects, timeout);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            SessionStateStoreData item = CreateNewStoreData(context, timeout);
            byte[] buffer = Serialize((SessionStateItemCollection)item.Items);

            lock (GetLock(id))
            {
                using (var store = _storeFactory.Get())
                {
                    SessionStateData data = store.Get(id);
                    if (data == null)
                    {
                        data = new SessionStateData
                        {
                            SessionId = id,
                            Timeout = timeout,
                            Expires = DateTime.Now.AddMinutes(timeout),
                            ItemSize = buffer.Length,
                            Payload = buffer
                        };
                        store.Save(data);
                    }
                }
            }
        }

        public override void Dispose()
        {
            _storeFactory.Dispose();
        }

        public override void EndRequest(HttpContext context)
        {
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            locked = false;
            lockAge = TimeSpan.FromSeconds(0);
            lockId = 0;
            actions = SessionStateActions.InitializeItem;

            using (var store = _storeFactory.Get())
            {
                SessionStateData data = store.Get(id);
                if (data != null)
                {
                    locked = data.Locked;
                    lockAge = DateTime.Now - data.LockDate;
                    lockId = data.LockCookie;
                    actions = SessionStateActions.None;
                    return new SessionStateStoreData(Deserialize(data.Payload), SessionStateUtility.GetSessionStaticObjects(context), data.Timeout);
                }
            }

            return null;
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            locked = false;
            lockAge = TimeSpan.FromSeconds(0);
            lockId = 0;
            actions = SessionStateActions.InitializeItem;

            lock (GetLock(id))
            {
                using (var store = _storeFactory.Get())
                {
                    SessionStateData data = store.Get(id);
                    if (data != null)
                    {
                        locked = data.Locked;
                        lockAge = TimeSpan.FromSeconds(0);
                        lockId = data.LockCookie;
                        actions = SessionStateActions.None;

                        data.Locked = true;
                        data.LockDate = DateTime.Now;
                        data.Initialized = true;

                        if (locked)
                        {
                            data.LockCookie = data.LockCookie + 1;
                        }

                        store.Save(data);

                        return new SessionStateStoreData(Deserialize(data.Payload), SessionStateUtility.GetSessionStaticObjects(context), data.Timeout);
                    }
                }

                return null;
            }
        }

        public override void InitializeRequest(HttpContext context)
        {
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            lock (GetLock(id))
            {
                using (var store = _storeFactory.Get())
                {
                    SessionStateData data = store.Get(id);
                    if (data != null && data.LockCookie == (int)lockId)
                    {
                        data.Expires = DateTime.Now.AddMinutes(data.Timeout);
                        data.Locked = false;
                        store.Save(data);
                    }
                }
            }
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            lock (GetLock(id))
            {
                using (var store = _storeFactory.Get())
                {
                    store.Delete(id);
                }
            }

            TryRemoveLock(id);
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            lock (GetLock(id))
            {
                using (var store = _storeFactory.Get())
                {
                    SessionStateData data = store.Get(id);
                    if (data != null)
                    {
                        data.Expires = DateTime.Now.AddMinutes(data.Timeout);
                        store.Save(data);
                    }
                }
            }
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            try
            {
                lock (GetLock(id))
                {
                    int lockCookie = (int)lockId;
                    byte[] buffer = Serialize((SessionStateItemCollection)item.Items);

                    var data = new SessionStateData
                    {
                        SessionId = id,
                        ItemSize = buffer.Length,
                        LockCookie = lockCookie,
                        Payload = buffer
                    };

                    if (newItem)
                    {
                        data.Created = DateTime.UtcNow;
                        data.Timeout = item.Timeout;
                        data.Expires = DateTime.UtcNow.AddMinutes(item.Timeout);
                    }

                    using (var store = _storeFactory.Get())
                    {
                        store.Save(data);
                    }
                }
            }
            catch
            {
                if (!newItem)
                {
                    // Release the exclusiveness of the existing item.
                    ReleaseItemExclusive(context, id, lockId);
                }
                throw;
            }
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        static Lock GetLock(string id)
        {
            return Locks.GetOrAdd(id, new Lock(id));
        }

        static void TryRemoveLock(string id)
        {
            Lock value;
            Locks.TryRemove(id, out value);
        }

        static byte[] Serialize(SessionStateItemCollection obj)
        {
            if (obj == null)
                return null;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                obj.Serialize(writer);
                return ms.ToArray();
            }
        }

        static SessionStateItemCollection Deserialize(byte[] buffer)
        {
            if (buffer == null)
                return null;

            using (var ms = new MemoryStream(buffer))
            using (var reader = new BinaryReader(ms))
            {
                return SessionStateItemCollection.Deserialize(reader);
            }
        }
    }
}
