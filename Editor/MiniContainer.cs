using System;
using System.Collections.Generic;
using System.Linq;

namespace SnivelerCode.SemanticSearch.Editor
{
    /// <summary>Minimal constructor-injection container contract.</summary>
    public interface IMiniContainer
    {
        /// <summary>Resolves the registered singleton of T.</summary>
        public T Resolve<T>();
        /// <summary>Creates a new instance of T with injected dependencies.</summary>
        public T Create<T>();
        /// <summary>Registers T and its interfaces for injection.</summary>
        public void Bind<T>() where T : class;
    }

    /// <summary>Default container with lazy singletons and constructor injection.</summary>
    public sealed class MiniContainer : IMiniContainer, IDisposable
    {
        private readonly Dictionary<Type, List<Registration>> _registrations = new();
        private bool _isDisposed;

        /// <summary>Creates the container and registers itself.</summary>
        public MiniContainer()
        {
            AddMap(typeof(IMiniContainer), new Registration
            {
                Type = GetType(),
                Instance = this
            });
        }

        /// <summary>Creates an instance of T.</summary>
        public T Create<T>() => (T) Create(typeof(T));

        /// <summary>Registers TImpl and every interface it implements.</summary>
        public void Bind<TImpl>() where TImpl : class
        {
            var implementationType = typeof(TImpl);
            var reg = new Registration {Type = implementationType};
            AddMap(implementationType, reg);

            var interfaces = implementationType.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                AddMap(@interface, reg);
            }
        }

        /// <summary>Adds a registration under a key type.</summary>
        private void AddMap(Type type, Registration reg)
        {
            if (!_registrations.ContainsKey(type))
                _registrations[type] = new List<Registration>();

            _registrations[type].Add(reg);
        }

        /// <summary>Resolves the newest registration of T.</summary>
        public T Resolve<T>() => (T) Resolve(typeof(T));

        /// <summary>Returns every registered instance of T.</summary>
        public IEnumerable<T> ResolveAll<T>()
        {
            var type = typeof(T);
            if (!_registrations.TryGetValue(type, out var list))
                return Enumerable.Empty<T>();

            return list.Select(reg => (T) GetInstance(reg));
        }

        /// <summary>Returns the newest registration of a type.</summary>
        private object Resolve(Type type)
        {
            if (!_registrations.TryGetValue(type, out var list))
                throw new Exception($"Type {type.Name} not registered.");

            return GetInstance(list.Last());
        }

        /// <summary>Returns the cached instance, creating it on first use.</summary>
        private object GetInstance(Registration reg)
        {
            if (reg.Instance != null) return reg.Instance;
            reg.Instance = CreateInstance(reg.Type);
            return reg.Instance;
        }

        /// <summary>Creates an instance through the public factory.</summary>
        private object CreateInstance(Type type) => Create(type);

        /// <summary>Instantiates a type, injecting constructor parameters and arrays.</summary>
        public object Create(Type type)
        {
            CheckDisposed();

            if (type.IsInterface || type.IsAbstract)
                throw new Exception($"Cannot create an abstract class: {type.Name}");

            var constructor =
                type.GetConstructors().FirstOrDefault()
                ?? throw new Exception($"The type {type.Name} has no public constructors.");

            var parameters = constructor.GetParameters();
            object[] parameterValues = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                if (paramType.IsArray)
                {
                    var elementType = paramType.GetElementType()!;
                    var instances = ResolveAllInternal(elementType).ToList();
                    var array = Array.CreateInstance(elementType, instances.Count);
                    for (int j = 0; j < instances.Count; j++)
                    {
                        array.SetValue(instances[j], j);
                    }

                    parameterValues[i] = array;
                }
                else
                {
                    parameterValues[i] = Resolve(paramType);
                }
            }

            return Activator.CreateInstance(type, parameterValues);
        }

        /// <summary>Materialises all registrations of a type.</summary>
        private IEnumerable<object> ResolveAllInternal(Type type)
        {
            if (!_registrations.TryGetValue(type, out var list))
                return Enumerable.Empty<object>();

            return list.Select(GetInstance);
        }

        /// <summary>One registration: implementation type plus optional instance.</summary>
        private sealed class Registration
        {
            public Type Type { get; set; }
            public object Instance { get; set; }
        }

        /// <summary>Throws when the container is already disposed.</summary>
        private void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(MiniContainer));
        }

        /// <summary>Disposes owned instances and clears registrations.</summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            var instances = _registrations.Values
                .SelectMany(x => x)
                .Where(x => x.Instance != this)
                .Select(x => x.Instance)
                .OfType<IDisposable>()
                .Distinct();

            foreach (var i in instances) i.Dispose();
            _registrations.Clear();
            _isDisposed = true;
        }
    }
}
