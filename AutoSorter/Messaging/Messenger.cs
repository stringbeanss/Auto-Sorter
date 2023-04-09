using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace AutoSorter.Messaging
{
    public class Messenger
    {
        public static readonly Messenger Default = new Messenger();

        private Dictionary<Type, List<IRecipient>> mi_recipients = new Dictionary<Type, List<IRecipient>>();

        public void Register<TMessage>(IRecipient<TMessage> _recipient) where TMessage : IMessage
            => Register(_recipient, typeof(TMessage));

        public void RegisterAll(IRecipient _recipient)
        {
            var recs = _recipient.GetType().GetInterfaces().Where(_o => typeof(IRecipient).IsAssignableFrom(_o)).ToArray();
            var messageTypes = recs.SelectMany(_o => _o.GenericTypeArguments).Where(_o => _o != null && typeof(IMessage).IsAssignableFrom(_o));
            foreach (var type in messageTypes)
            {
                Register(_recipient, type);
            }
        }

        public void RegisterAll(IRecipient _recipient, params Type[] _messageTypes)
        {
            foreach(var type in _messageTypes)
            {
                Register(_recipient, type);
            }
        }

        private void Register(IRecipient _recipient, Type _messageType)
        {
            if (!typeof(IMessage).IsAssignableFrom(_messageType)) throw new InvalidOperationException($"Invalid message type {_messageType}. Must inherit {nameof(IMessage)}.");

            if (mi_recipients.ContainsKey(_messageType))
            {
                mi_recipients[_messageType].Add(_recipient);
                return;
            }
            mi_recipients.Add(_messageType, new List<IRecipient>() { _recipient });
        }

        public void Unregister<TMessage>(IRecipient<TMessage> _recipient) where TMessage : IMessage
        {
            if (mi_recipients.ContainsKey(typeof(TMessage)))
            {
                mi_recipients[typeof(TMessage)].Remove(_recipient);
                if (mi_recipients[typeof(TMessage)].Count == 0)
                {
                    mi_recipients.Remove(typeof(TMessage));
                }
            }
        }

        public void UnregisterAll(IRecipient _recipient)
        {
            foreach(var recipients in mi_recipients.Values)
            {
                for(int i = 0; i < recipients.Count; ++i)
                {
                    if (recipients[i] == _recipient) recipients.RemoveAt(i);
                    --i;
                }
            }
        }

        public void Send<TMessage>() where TMessage : IMessage, new()
            => Send(new TMessage());

        public TResult Send<TMessage, TResult>() where TMessage : ResultMessage<TResult>, new()
            => Send<TMessage, TResult>(new TMessage());

        public void Send<TMessage>(TMessage _message) where TMessage : IMessage
            => Call(_message);

        public TResult Send<TMessage, TResult>(TMessage _message) where TMessage : ResultMessage<TResult>
        {
            Call(_message);
            return _message.Result;
        }

        private void Call<TMessage>(TMessage _message) where TMessage : IMessage
        {
            if (mi_recipients.ContainsKey(_message.GetType()))
            {
                foreach (var recipient in mi_recipients[_message.GetType()])
                {
                    ((IRecipient<TMessage>)recipient).Receive(_message);
                }
            }
        }
    }

    public interface IRecipient {}

    public interface IRecipient<TMessage> : IRecipient where TMessage : IMessage
    {
        void Receive(TMessage _message);
    }

    public interface IMessage {}

    public class ResultMessage<TResult> : IMessage
    {
        public TResult Result { get; private set; }

        public void SetResult(TResult _result)
        {
            if(Result != null)
            {
                throw new InvalidOperationException("The result for this message has already been set. Result can only be set once per message.");
            }
            Result = _result;
        }
    }
}
