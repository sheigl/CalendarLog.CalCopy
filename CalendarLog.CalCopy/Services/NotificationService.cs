using ElectronNET.API;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private ConcurrentDictionary<string, List<Action<object>>> _subscribers;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _subscribers = new ConcurrentDictionary<string, List<Action<object>>>();
            _logger = logger;
        }

        public void Publish(string channel, object args)
        {
            if (_subscribers.TryGetValue(channel, out List<Action<object>> subscribers))
            {
                subscribers.ForEach(subscriber => subscriber(args));
            }

            _logger.LogDebug($"[{channel}] {JsonConvert.SerializeObject(args)}");
        }

        public void Subscribe(string channel, Action<object> action)
        {
            _subscribers.AddOrUpdate(channel, key => new List<Action<object>> { action }, (key, subscribers) => 
            {
                if (!subscribers.Contains(action))
                {
                    subscribers.Add(action);
                }

                return subscribers;
            });
        }
    }
}
