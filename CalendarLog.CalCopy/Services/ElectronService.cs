using ElectronNET.API;
using ElectronNET.API.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public class ElectronService : IElectronService
    {
        private bool _isSelecting = false;

        public IElectronService RegisterListener(string channel, Action<object> listener)
        {
            if (HybridSupport.IsElectronActive)
            {
                Electron.IpcMain.On(channel, listener);
            }

            return this;
        }

        public async Task<string> SelectDialogAsync(OpenDialogProperty property, string title = null, string message = null)
        {
            if (HybridSupport.IsElectronActive)
            {
                if (!_isSelecting)
                {
                    _isSelecting = true;

                    OpenDialogOptions options = new OpenDialogOptions
                    {
                        Properties = new OpenDialogProperty[]
                        {
                            property
                        },
                        Title = title ?? "Select something",
                        Message = message ?? "Please select something"
                    };

                    string[] files = await Electron.Dialog.ShowOpenDialogAsync(await GetFocusedWindowAsync(), options);

                    _isSelecting = false;

                    return files.FirstOrDefault();
                }
            }

            return string.Empty;
        }

        private async Task<BrowserWindow> GetFocusedWindowAsync()
        {
            if (HybridSupport.IsElectronActive)
            {
                BrowserWindow focused = default(BrowserWindow);

                foreach (var window in Electron.WindowManager.BrowserWindows)
                {
                    if (await window.IsFocusedAsync())
                    {
                        focused = window;
                        break;
                    }
                }

                return focused;
            }
            else
            {
                return default(BrowserWindow);
            }
        }
    }
}
