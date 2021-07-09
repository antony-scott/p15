using p15.Core.Messages;
using System;
using System.Collections.Generic;

namespace p15.Core.Services
{
    public interface IMessagingService
    {
        void SendMessage<T>(T message) where T : new();
        void Subscribe<T>(Action<T> action) where T : new();
    }

    public class MessagingService : IMessagingService
    {
        private Dictionary<Type, List<Action<object>>> _subscriptions = new Dictionary<Type, List<Action<object>>>();

        public void SendMessage<T>(T message) where T : new()
        {
            if (_subscriptions.TryGetValue(message.GetType(), out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        subscriber.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        SendMessage(new TraceOutputMessage { Trace = ex.Message + " - " + ex.StackTrace });
                    }
                }
            }
        }

        public void Subscribe<T>(Action<T> action) where T : new()
        {
            if (!_subscriptions.TryGetValue(typeof(T), out var subscribers))
            {
                subscribers = new List<Action<object>>();
                _subscriptions.Add(typeof(T), subscribers);
            }
       
            subscribers.Add(obj => action((T)obj));
        }
    }
}
