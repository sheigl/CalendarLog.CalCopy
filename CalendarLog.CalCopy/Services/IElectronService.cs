using ElectronNET.API;
using ElectronNET.API.Entities;
using System;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public interface IElectronService
    {
        IElectronService RegisterListener(string channel, Action<object> listener);
        Task<string> SelectDialogAsync(OpenDialogProperty property, string title = null, string message = null);
    }
}