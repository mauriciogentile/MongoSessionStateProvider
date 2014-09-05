using System;

namespace AspNet.Session.MongoSessionStateProvider
{
    public sealed class Lock
    {
        private readonly string _name;

        public string Name { get { return _name; } }

        public Lock(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            _name = name;
        }
    }
}
