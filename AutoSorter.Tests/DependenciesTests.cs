using AutoSorter.DI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace pp.RaftMods.AutoSorter.Tests
{
    [TestClass]
    public class DependenciesTests
    {
        [TestMethod]
        public void Bind_Success()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubImplementationNoContruct>();
        }

        [TestMethod]
        public void Bind_AlreadyRegistered_ShouldThrowException()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubImplementationNoContruct>();
            kernel.Bind<StubInterface, StubImplementationEmptyConstruct>();

            var resolve = kernel.Resolve<StubInterface>();
            Assert.IsInstanceOfType(resolve, typeof(StubImplementationEmptyConstruct));    
        }

        [TestMethod]
        public void Resolve_EmptyType_ShouldThrowException()
        {
            var kernel = new Dependencies();
            Assert.ThrowsException<ArgumentNullException>(() => kernel.Resolve(null));
        }

        [TestMethod]
        public void Resolve_NoConstructor_CreateDefault()
        {
            var kernel = new Dependencies();
            Assert.IsNotNull(kernel.Resolve<StubImplementationNoContruct>());
        }

        [TestMethod]
        public void Resolve_EmptyConstructor_CreateDefault()
        {
            var kernel = new Dependencies();
            Assert.IsNotNull(kernel.Resolve<StubImplementationEmptyConstruct>());
        }

        [TestMethod]
        public void Resolve_PrivateConstructor_CreateDefault()
        {
            var kernel = new Dependencies();
            Assert.ThrowsException<InjectionException>(() => kernel.Resolve(typeof(StubImplementationNonPublicConstruct)));
        }

        [TestMethod]
        public void Resolve_CanInjectConstructor_CreateWithInjected()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubImplementationNoContruct>();

            var resolve = kernel.Resolve<StubImplementation>();
            Assert.IsInstanceOfType(resolve.Member, typeof(StubImplementationNoContruct));
            Assert.IsNotNull(resolve);
        }

        [TestMethod]
        public void Resolve_AsSingleton_ShouldCreateSameInstance()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubSingleton>().AsSingleton();

            var resolve1 = kernel.Resolve<StubSingleton>();
            var resolve2 = kernel.Resolve<StubSingleton>();

            Assert.AreEqual(resolve1.ID, resolve2.ID);
        }

        [TestMethod]
        public void Resolve_AsTransient_ShouldCreateDifferentInstances()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubSingleton>().AsTransient();

            var resolve1 = kernel.Resolve<StubSingleton>();
            var resolve2 = kernel.Resolve<StubSingleton>();

            Assert.AreNotEqual(resolve1.ID, resolve2.ID);
        }

        [TestMethod]
        public void Resolve_SelfBindWithInjection_ShouldInject()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubImplementationNoContruct>();
            kernel.Bind<StubSingletonWithInjection>();

            var resolve1 = kernel.Resolve<StubSingletonWithInjection>();

            Assert.IsNotNull(resolve1.Member);
            Assert.IsInstanceOfType(resolve1.Member, typeof(StubImplementationNoContruct));
        }

        [TestMethod]
        public void Resolve_BindToConstant_ShouldReturnConstant()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface>().ToConstant(new StubImplementationEmptyConstruct());

            var resolve = kernel.Resolve<StubInterface>();

            Assert.IsNotNull(resolve);
            Assert.IsInstanceOfType(resolve, typeof(StubImplementationEmptyConstruct));
        }

        [TestMethod]
        public void Resolve_BindToMismatchingConstant_ShouldReturnConstant()
        {
            var kernel = new Dependencies();
            Assert.ThrowsException<InvalidOperationException>(() => kernel.Bind<StubInterface>().ToConstant(new StubImplementation(null)));
        }

        [TestMethod]
        public void Resolve_NestedConstructorInjection_CreateWithInjected()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, NestedStubImplementation>();
            kernel.Bind<NestedStubInterface, NestedResultStubImplementation>();

            var resolve = kernel.Resolve<StubImplementation>();
            Assert.IsInstanceOfType(resolve.Member.Member, typeof(NestedResultStubImplementation));
            Assert.IsNotNull(resolve);
        }

        [TestMethod]
        public void Resolve_NoMatchingInjections_ThrowsException()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, NestedStubImplementation>();

            Assert.ThrowsException<ArgumentException>(() => kernel.Resolve<StubImplementation>());
        }

        [TestMethod]
        public void Resolve_ConfigureTransientTwice_ThrowsException()
        {
            var kernel = new Dependencies();
            var bind = kernel.Bind<StubInterface, NestedStubImplementation>();
            bind.AsTransient();
            Assert.ThrowsException<InvalidOperationException>(() => bind.AsTransient());
        }

        [TestMethod]
        public void Resolve_ConfigureSingletonTwice_ThrowsException()
        {
            var kernel = new Dependencies();
            var bind = kernel.Bind<StubInterface, NestedStubImplementation>();
            bind.AsSingleton();
            Assert.ThrowsException<InvalidOperationException>(() => bind.AsSingleton());
        }

        [TestMethod]
        public void Resolve_ConfigureConstantTwice_ThrowsException()
        {
            var kernel = new Dependencies();
            var bind = kernel.Bind<StubInterface, NestedStubImplementation>();
            bind.ToConstant(null);
            Assert.ThrowsException<InvalidOperationException>(() => bind.ToConstant(null));
        }

        [TestMethod]
        public void Resolve_ResolveNullConstant_ReturnsNull()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface>().ToConstant(null);

            var resolve = kernel.Resolve<StubInterface>();
            Assert.IsNull(resolve);
        }

        [TestMethod]
        public void Resolve_SelfInterface_ThrowsException()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface>();

            Assert.ThrowsException<InvalidOperationException>(() => kernel.Resolve<StubInterface>());
        }

        [TestMethod]
        public void Resolve_InterfaceIsTarget_ThrowsException()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubInterfaceDerivingFromInterface>();

            Assert.ThrowsException<InvalidOperationException>(() => kernel.Resolve<StubInterface>());
        }

        [TestMethod]
        public void Resolve_SelfClass_ShouldReturnInstance()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubImplementationNoContruct>();

            var resolve = kernel.Resolve<StubImplementationNoContruct>();
            Assert.IsNotNull(resolve);
            Assert.IsInstanceOfType(resolve, typeof(StubImplementationNoContruct));
        }

        [TestMethod]
        public void Resolve_InstanceParameter_ShouldInjectValue()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubWithValueConstructor>("someValue");

            var resolve = kernel.Resolve<StubWithValueConstructor>();
            Assert.AreEqual("someValue", resolve.Value);
        }

        [TestMethod]
        public void Resolve_InstanceParameterInterfaceBound_ShouldInjectValue()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubWithValueConstructor>("someValue");

            var resolve = kernel.Resolve<StubInterface>();
            Assert.IsInstanceOfType(resolve, typeof(StubWithValueConstructor));
            Assert.AreEqual("someValue", ((StubWithValueConstructor)resolve).Value);
        }

        [TestMethod]
        public void Call_NoSource_ThrowsException()
        {
            var kernel = new Dependencies();
            Assert.ThrowsException<ArgumentNullException>(() => kernel.Call(null, null));
        }

        [TestMethod]
        public void Call_NoMethod_ThrowsException()
        {
            var kernel = new Dependencies();
            Assert.ThrowsException<ArgumentException>(() => kernel.Call(new object(), ""));
        }

        [TestMethod]
        public void Call_MethodNotFound_ThrowsException()
        {
            var kernel = new Dependencies();

            Assert.ThrowsException<ArgumentException>(() => kernel.Call(new StubWithInjectedMethods(), "somethingWrong"));
        }

        [TestMethod]
        public void Call_MethodPrivate_ThrowsException()
        {
            var kernel = new Dependencies();

            Assert.ThrowsException<ArgumentException>(() => kernel.Call(new StubWithInjectedMethods(), "IsPrivateNotFound"));
        }

        [TestMethod]
        public void Call_MethodStatic_ThrowsException()
        {
            var kernel = new Dependencies();

            Assert.ThrowsException<ArgumentException>(() => kernel.Call(new StubWithInjectedMethods(), nameof(StubWithInjectedMethods.IsStaticNotFound)));
        }

        [TestMethod]
        public void Call_InjectVoidMethod_ReturnsInstance()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubImplementationNoContruct>();

            var instance = new StubWithInjectedMethods();
            _ = kernel.Call(instance, nameof(StubWithInjectedMethods.DoInvokeVoid));

            Assert.IsNotNull(instance.Member);
            Assert.IsInstanceOfType(instance.Member, typeof(StubImplementationNoContruct));
        }

        [TestMethod]
        public void Call_InjectReturnMethod_ReturnsNameOfInjectedType()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubImplementationNoContruct>();

            var instance = new StubWithInjectedMethods();
            string typeName = kernel.Call<string>(instance, nameof(StubWithInjectedMethods.DoInvokeReturns));

            Assert.AreEqual(typeof(StubImplementationNoContruct).Name, typeName);
        }

        [TestMethod]
        public void Call_InjectMethod_ParameterTypeNotBound()
        {
            var kernel = new Dependencies();
            kernel.Bind<StubInterface, StubImplementationNoContruct>();

            Assert.ThrowsException<InvalidOperationException>(() => kernel.Call(new StubWithInjectedMethods(), nameof(StubWithInjectedMethods.DoInvokeParameterNotFound)));
        }

        #region STUBS
        interface StubInterface 
        {
            NestedStubInterface Member { get; }
        }

        class StubImplementationNoContruct : StubInterface
        { 
            NestedStubInterface StubInterface.Member => throw new NotImplementedException();
        }

        class StubImplementationEmptyConstruct : StubInterface
        {
            NestedStubInterface StubInterface.Member => throw new NotImplementedException();

            public StubImplementationEmptyConstruct()
            {
                
            }
        }

        class StubImplementationNonPublicConstruct : StubInterface
        {
            NestedStubInterface StubInterface.Member => throw new NotImplementedException();

            private StubImplementationNonPublicConstruct()
            {

            }
        }
    
        class StubImplementation
        {
            public StubInterface Member { get; private set; }

            public StubImplementation(StubInterface _interface)
            {
                Member = _interface;
            }
        }

        interface NestedStubInterface { }

        class NestedStubImplementation : StubInterface
        {
            public NestedStubInterface Member { get; private set; }

            public NestedStubImplementation(NestedStubInterface _interface)
            {
                Member = _interface;
            }
        }

        class NestedResultStubImplementation : NestedStubInterface { }
       
        interface StubSingletonInterface { }

        class StubSingleton : StubSingletonInterface 
        {
            public System.Guid ID { get; private set; }

            public StubSingleton()
            {
                ID = System.Guid.NewGuid();
            }
        }

        class StubSingletonWithInjection : StubSingletonInterface
        {
            public System.Guid ID { get; private set; }
            public StubInterface Member { get; private set; }

            public StubSingletonWithInjection(StubInterface member)
            {
                ID = System.Guid.NewGuid();
                Member = member;
            }
        }

        interface StubInterfaceDerivingFromInterface : StubInterface { }
        
        class StubWithInjectedMethods
        {
            public StubInterface Member;

            public string DoInvokeReturns(StubInterface _injectMe)
            {
                return _injectMe.GetType().Name;
            }

            public void DoInvokeVoid(StubInterface _injectMe)
            {
                Member = _injectMe;
            }

            public void DoInvokeParameterNotFound(StubSingleton _injectMe)
            {
            }

            private void IsPrivateNotFound()
            {

            }

            public static void IsStaticNotFound()
            {

            }
        }
       
        class StubWithValueConstructor : StubInterface
        {
            public string Value;

            public StubWithValueConstructor(string _value)
            {
                Value = _value;
            }

            public NestedStubInterface Member => throw new NotImplementedException();
        }
        #endregion
    }
}
