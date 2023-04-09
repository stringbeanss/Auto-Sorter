using AutoSorter.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace pp.RaftMods.AutoSorter.Tests
{
    [TestClass]
    public class MessengerTests
    {
        [TestMethod]
        public void Send_SimpleMessage_ShouldReceiveMessage()
        {
            var messenger = CreateMessenger();
            var receiver = new TestReceiver();
            messenger.Register(receiver);

            messenger.Send<TestMessage>();

            Assert.IsNotNull(receiver.Received);
            Assert.IsInstanceOfType(receiver.Received, typeof(TestMessage));
        }

        [TestMethod]
        public void Send_NotMatchingMessage_ShouldNotReceiveMessage()
        {
            var messenger = CreateMessenger();
            var receiver = new TestResultReceiver("");
            messenger.Register(receiver);

            messenger.Send<TestMessage>();

            Assert.IsNull(receiver.Received);
        }

        [TestMethod]
        public void Send_ToMultiReceiver_ShouldReceiveAllMessages()
        {
            var messenger = CreateMessenger();
            var receiver = new TestMultiReceiver();

            messenger.Register<TestMessage>(receiver);
            messenger.Register<SecondTestMessage>(receiver);

            messenger.Send<TestMessage>();
            messenger.Send<SecondTestMessage>();

            Assert.IsNotNull(receiver.First);
            Assert.IsNotNull(receiver.Second);
        }

        [TestMethod]
        public void Send_SimpleWithMultipleReceivers_AllShouldReceiveMessage()
        {
            var messenger = new Messenger();

            TestReceiver[] receivers = new[]
            {
                new TestReceiver(),
                new TestReceiver(),
                new TestReceiver()
            };

            foreach(var r in receivers)
            {
                messenger.Register(r);
            }

            messenger.Send<TestMessage>();

            foreach (var r in receivers)
            {
                Assert.IsNotNull(r.Received);
                Assert.IsInstanceOfType(r.Received, typeof(TestMessage));
            }
        }
        
        [TestMethod]
        public void Send_ResultMessage_ShouldReceiveMessage()
        {
            var messenger = CreateMessenger();
            var receiver = new TestResultReceiver("someResult");
            messenger.Register(receiver);

            string result = messenger.Send<TestResultMessage, string>(new TestResultMessage());

            Assert.IsNotNull(result);
            Assert.AreEqual("someResult", result);
        }

        [TestMethod]
        public void Send_ResultSetTwice_ShouldThrowExceptionSecond()
        {
            var messenger = CreateMessenger();

            var receiverOne = new TestResultReceiver("someResult");
            var receiverTwo = new TestResultReceiver("someResult");

            messenger.Register(receiverOne);
            messenger.Register(receiverTwo);

            Assert.ThrowsException<InvalidOperationException>(() => messenger.Send<TestResultMessage, string>(new TestResultMessage()));
        }

        [TestMethod]
        public void Unregister_SimpleMessage_ShouldNotBeReceived()
        {
            var messenger = CreateMessenger();

            var receiver = new TestReceiver();

            messenger.Register(receiver);
            messenger.Unregister(receiver);

            messenger.Send<TestMessage>();

            Assert.IsNull(receiver.Received);
        }

        [TestMethod]
        public void RegisterAll_MultiReceiverWithTypes_ShouldReceiveAll()
        {
            var messenger = CreateMessenger();
            var receiver = new TestMultiReceiver();

            messenger.RegisterAll(receiver, typeof(TestMessage), typeof(SecondTestMessage));

            messenger.Send<TestMessage>();
            messenger.Send<SecondTestMessage>();

            Assert.IsNotNull(receiver.First);
            Assert.IsNotNull(receiver.Second);
        }

        [TestMethod]
        public void RegisterAll_MultiReceiverReflection_ShouldReceiveAll()
        {
            var messenger = CreateMessenger();
            var receiver = new TestMultiReceiver();

            messenger.RegisterAll(receiver);

            messenger.Send<TestMessage>();
            messenger.Send<SecondTestMessage>();

            Assert.IsNotNull(receiver.First);
            Assert.IsNotNull(receiver.Second);
        }

        [TestMethod]
        public void RegisterAll_InvalidMessage_ShouldThrowException()
        {
            var messenger = CreateMessenger();
            Assert.ThrowsException<InvalidOperationException>(() => messenger.RegisterAll(new TestMultiReceiver(), typeof(object)));
        }

        [TestMethod]
        public void Unregister_ResultMessage_ShouldNotBeReceived()
        {
            var messenger = CreateMessenger();
            var receiver = new TestResultReceiver("someResult");

            messenger.Register(receiver);
            messenger.Unregister(receiver);

            string result = messenger.Send<TestResultMessage, string>(new TestResultMessage());

            Assert.IsNull(result);
        }
        
        [TestMethod]
        public void Unregister_RemoveOnMultiReceiver_ShouldOnlyReceiveRemaining()
        {
            var messenger = CreateMessenger();
            var receiver = new TestMultiReceiver();

            messenger.Register<TestMessage>(receiver);
            messenger.Register<SecondTestMessage>(receiver);

            messenger.Unregister<TestMessage>(receiver);

            messenger.Send<TestMessage>();
            messenger.Send<SecondTestMessage>();

            Assert.IsNull(receiver.First);
            Assert.IsNotNull(receiver.Second);
        }

        [TestMethod]
        public void UnregisterAll_MultiReceiver_ShouldNotBeReceive()
        {
            var messenger = CreateMessenger();
            var receiver = new TestMultiReceiver();

            messenger.Register<TestMessage>(receiver);
            messenger.Register<SecondTestMessage>(receiver);

            messenger.UnregisterAll(receiver);

            messenger.Send<TestMessage>();
            messenger.Send<SecondTestMessage>();

            Assert.IsNull(receiver.First);
            Assert.IsNull(receiver.Second);
        }

        private Messenger CreateMessenger()
        {
            return new Messenger();
        }

        #region FAKES
        private class TestMessage : IMessage { }

        private class SecondTestMessage : IMessage { }

        private class TestResultMessage : ResultMessage<string> { }

        private class TestReceiver : IRecipient<TestMessage>
        {
            public IMessage Received { get; private set; }

            public void Receive(TestMessage _message)
            {
                Received = _message;
            }
        }

        private class TestMultiReceiver :   IRecipient<TestMessage>,
                                            IRecipient<SecondTestMessage>
        {
            public IMessage First { get; private set; }
            public IMessage Second { get; private set; }

            public void Receive(SecondTestMessage _message) => Second = _message;

            public void Receive(TestMessage _message) => First = _message;
        }

        private class TestResultReceiver : IRecipient<TestResultMessage>
        {
            public IMessage Received { get; private set; }

            private readonly string mi_result;

            public TestResultReceiver(string _result)
            {
                mi_result = _result;
            }

            public void Receive(TestResultMessage _message)
            {
                Received = _message;
                _message.SetResult(mi_result);
            }
        }
        #endregion
    }
}