using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoSorter.IOC
{
    public class Dependencies
    {
        private Dictionary<Type, DependencyBinder> m_resolverTable = new Dictionary<Type, DependencyBinder>();

        public IDependencyBinder Bind<TInterface, TImplementation>(params object[] _parameter) where TImplementation : TInterface
        {
            if(m_resolverTable.ContainsKey(typeof(TInterface)))
            {
                m_resolverTable.Remove(typeof(TInterface));
            }
            var binder = new DependencyBinder(typeof(TInterface), typeof(TImplementation), _parameter);
            m_resolverTable.Add(binder.Source, binder);
            return binder;
        }

        public IDependencyBinder Bind<TImplementation>(params object[] _parameter)
        {
            if (m_resolverTable.ContainsKey(typeof(TImplementation)))
            {
                m_resolverTable.Remove(typeof(TImplementation));
            }
            var binder = new DependencyBinder(typeof(TImplementation), typeof(TImplementation), _parameter);
            binder.ToSelf();
            m_resolverTable.Add(binder.Source, binder);
            return binder;
        }

        public T Resolve<T>() where T : class => (T)Resolve(typeof(T));

        public object Resolve(Type _type)
        {
            if (_type == null) throw new ArgumentNullException("_type");

            var constructors = _type.GetConstructors();
            if (constructors.Length == 0)
            {
                if(m_resolverTable.ContainsKey(_type))
                {
                    return m_resolverTable[_type].Create(this, null);
                }
                return CreateInstance(_type);
            }

            var validConstructor = constructors.FirstOrDefault(_o => _o.GetParameters().All(_n => m_resolverTable.ContainsKey(_n.ParameterType)));
            if(validConstructor == null) 
            {
                throw new ArgumentException($"No valid constructor found for injection on {_type.Name}");
            }
            var parameter = validConstructor.GetParameters();
            if (m_resolverTable.ContainsKey(_type))
            {
                return m_resolverTable[_type].Create(this, parameter);
            }
            return validConstructor.Invoke(parameter.Select(_o => m_resolverTable[_o.ParameterType].Create(this, null)).ToArray());
        }

        private object CreateInstance(Type _type, ParameterInfo[] _params = null)
        {
            try
            {
                return Activator.CreateInstance(_type, _params?.Select(_o => Resolve(_o.ParameterType)).ToArray());
            }
            catch(MissingMethodException)
            {
                throw new InjectionException($"Failed to find matching constructor on {_type.Name}");
            }
        }

        private class DependencyBinder : IDependencyBinder
        {
            public Type Source { get; private set; }
            public Type Target { get; private set; }

            public object Instance { get; private set; }

            private EDependencyType DependencyType { get; set; } = EDependencyType.NONE;

            private bool SelfBound { get; set; }
            private object[] Parameter { get; set; }

            public DependencyBinder(Type _source, Type _target, object[] parameter)
            {
                Source = _source;
                Target = _target;
                Parameter = parameter;
            }

            public object Create(Dependencies _dependencies, ParameterInfo[] _params)
            {
                if ((DependencyType == EDependencyType.SINGLETON && Instance != null) || 
                    DependencyType == EDependencyType.CONSTANT) 
                    return Instance;
                return (Instance = 
                    _params == null ? 
                        _dependencies.Resolve(Target) : 
                        _dependencies.CreateInstance(SelfBound ? Source : Target, _params));
            }

            public void AsTransient()
            {
                if (DependencyType != EDependencyType.NONE) throw new InvalidOperationException($"Calls to {nameof(AsTransient)}, {nameof(AsSingleton)}, {nameof(ToConstant)} are only allowed once per dependency.");
                DependencyType = EDependencyType.TRANSIENT;
            }

            public void AsSingleton()
            {
                if (DependencyType != EDependencyType.NONE) throw new InvalidOperationException($"Calls to {nameof(AsTransient)}, {nameof(AsSingleton)}, {nameof(ToConstant)} are only allowed once per dependency.");
                DependencyType = EDependencyType.SINGLETON;
            }

            public void ToSelf()
            {
                SelfBound = true;
            }

            public void ToConstant(object _value)
            {
                if (DependencyType != EDependencyType.NONE) throw new InvalidOperationException($"Calls to {nameof(AsTransient)}, {nameof(AsSingleton)}, {nameof(ToConstant)} are only allowed once per dependency.");
                if (_value != null && !Source.IsAssignableFrom(_value.GetType()))
                {
                    throw new InvalidOperationException($"Cannot bind to a constant which does not derive the binding type. Constant: {_value.GetType().Name} Binding: {Source.Name}");
                }
                DependencyType = EDependencyType.CONSTANT;
                Instance = _value;
            }

            private enum EDependencyType
            {
                NONE = 0,
                TRANSIENT = 1,
                CONSTANT = 2,
                SINGLETON = 3
            }
        }
    }
}
