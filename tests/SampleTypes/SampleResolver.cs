using System;
using System.Collections.Concurrent;
using MessageRouter.Interfaces;

namespace SampleTypes.CSharp
{
    public class SampleResolver : IResolver
    {
        private readonly ConcurrentDictionary<Type, object> _container;

        public SampleResolver()
        {
            _container = new ConcurrentDictionary<Type, object>();
        }

        public void AddType(Type type, object instance)
        {
            _container.GetOrAdd(type, instance);
        }

        public bool CanResolve(Type type)
        {
            return _container.ContainsKey(type);
        }

        public T Get<T>()
        {
            object output;
            _container.TryGetValue(typeof (T), out output);
            return (T)output;
        }

        public object Get(Type type)
        {
            object output;
            _container.TryGetValue(type, out output);
            return output;
        }
    }
}
