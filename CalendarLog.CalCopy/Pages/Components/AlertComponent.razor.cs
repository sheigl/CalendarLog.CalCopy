using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace CalendarLog.CalCopy.Pages.Components
{
    public partial class AlertComponent
    {
        private AlertOptions _options = new AlertOptions();

        [Parameter]
        public AlertOptions Options
        {
            get
            {
                return _options;
            }
            set
            {
                if (value != null)
                {
                    _options = value;
                    _options.PropertyChanged += OnSettingsChanged;
                }
            }
        }

        private Timer _autohideTimer;

        public void OnSettingsChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(AlertOptions.DismissIn):
                    
                    if (_options.DismissIn != null && _options.DismissIn.Value.TotalSeconds > 0)
                    {
                        if (_autohideTimer != null)
                        {
                            _autohideTimer.Dispose();
                            _autohideTimer = null;
                        }

                        _autohideTimer = new Timer(OnAutohide, _options, (int)_options.DismissIn.Value.TotalMilliseconds, Timeout.Infinite);                        
                    }

                    break;
                default:
                    break;
            }
        }

        private async void OnAutohide(object state)
        {
            Options.Show = false;
            await InvokeAsync(() => StateHasChanged());
        }

        public enum AlertTypes
        {
            danger,
            info,
            primary,
            success,
            warning
        }

        public class AlertOptions : INotifyPropertyChanged
        {
            private bool _show;

            public bool Show
            {
                get { return _show; }
                set 
                {
                    _show = value;
                    OnPropertyChanged(nameof(Show));
                }
            }

            private AlertTypes _alertType;

            public AlertTypes AlertType
            {
                get { return _alertType; }
                set 
                {
                    _alertType = value;
                    OnPropertyChanged(nameof(AlertType));
                }
            }


            private string _title;

            public string Title
            {
                get { return _title; }
                set 
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }

            private string _message;

            public string Message
            {
                get { return _message; }
                set 
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }

            private string _errorMessage;

            public string ErrorMessage
            {
                get { return _errorMessage; }
                set 
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }

            private TimeSpan? _dismissIn;

            public TimeSpan? DismissIn
            {
                get { return _dismissIn; }
                set 
                {
                    _dismissIn = value;
                    OnPropertyChanged(nameof(DismissIn));
                }
            }


            public event PropertyChangedEventHandler PropertyChanged;

            ~AlertOptions()
            {
                if (PropertyChanged != null && PropertyChanged.GetInvocationList().Any())
                {
                    foreach (PropertyChangedEventHandler del in PropertyChanged.GetInvocationList())
                    {
                        PropertyChanged -= del;
                    }
                }
            }

            public void OnPropertyChanged(string propName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
