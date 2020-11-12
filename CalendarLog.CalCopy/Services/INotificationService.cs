using System;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public interface INotificationService
    {
        void Publish(string channel, object args);
        void Subscribe(string channel, Action<object> action);
    }
}