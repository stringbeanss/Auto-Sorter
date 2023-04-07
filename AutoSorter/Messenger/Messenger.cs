using Mono.Security.Protocol.Ntlm;
using System;
using System.Collections.Generic;

namespace AutoSorter.Messaging
{
    public class Messenger
    {
        public static readonly Messenger Default = new Messenger();

        private Dictionary<Type, List<IRecipient>> mi_recipients = new Dictionary<Type, List<IRecipient>>();

        public delegate void MessageDelegate(IMessage _message);

        public void Register<TMessage>(IRecipient<TMessage> _receiver) where TMessage : IMessage
        {
            if(mi_recipients.ContainsKey(typeof(TMessage)))
            {
                mi_recipients[typeof(TMessage)].Add(_receiver);
                return;
            }
            mi_recipients.Add(typeof(TMessage), new List<IRecipient>() { _receiver });
        }

        public void Send<TMessage>() where TMessage : IMessage, new()
            => Send(new TMessage());

        public void Send<TMessage>(TMessage _message) where TMessage : IMessage
        {
            if (!mi_recipients.ContainsKey(_message.GetType())) return;
            foreach(var recipient in mi_recipients[_message.GetType()])
            {
                ((IRecipient<TMessage>)recipient).Receive(_message);
            }
        }
    }

    public interface IRecipient {}
    public interface IRecipient<TMessage> : IRecipient where TMessage : IMessage
    {
        void Receive(TMessage _message);
    }

    public interface IMessage
    {

    }
}
