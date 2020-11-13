using CalendarLog.CalCopy.Models;
using CalendarLog.CalCopy.Pages.Components;
using CalendarLog.CalCopy.Services;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Pages
{
    public partial class SettingsComponent
    {
        [Inject]
        public CalCopyDbContext Context { get; set; }

        [Inject]
        public IElectronService Electron { get; set; }

        public bool Loading { get; set; }
        public bool IsSaving { get; set; }
        public Settings Settings { get; set; }

        public AlertComponent.AlertOptions AlertOptions { get; set; }
            = new AlertComponent.AlertOptions();

        protected override async Task OnInitializedAsync()
        {
            Settings = await Context.Settings.FirstOrDefaultAsync() ?? new Settings();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                Electron.RegisterListener(Constants.Channels.OnMasterTemplateClickAsync, HandleOnMasterTemplateClickAsync)
                   .RegisterListener(Constants.Channels.OnProofingFolderClickAsync, HandleOnProofingFolderClickAsync)
                   .RegisterListener(Constants.Channels.OnWorkingCalendarFolderClickAsync, HandleOnWorkingCalendarFolderClickAsync);
            }
        }

        private async void HandleOnMasterTemplateClickAsync(object sender) =>
            await HandleDialogClickAsync(settings => settings.MasterTemplateFile, OpenDialogProperty.openFile);

        private async void HandleOnProofingFolderClickAsync(object sender) =>
            await HandleDialogClickAsync(settings => settings.ProofingFolder, OpenDialogProperty.openDirectory);

        private async void HandleOnWorkingCalendarFolderClickAsync(object sender) =>
            await HandleDialogClickAsync(settings => settings.WorkingCalendarFolder, OpenDialogProperty.openDirectory);

        private async Task HandleDialogClickAsync(Expression<Func<Settings, string>> selector, OpenDialogProperty openDialogProperty)
        {
            MemberExpression memExp = selector.Body as MemberExpression;
            Action<object> setMethod = value => typeof(Settings).GetProperty(memExp.Member.Name)
                .GetSetMethod()
                .Invoke(Settings, new[] { value });

            setMethod(await Electron.SelectDialogAsync(openDialogProperty));
            await InvokeAsync(StateHasChanged);
        }

        private async Task SaveAsync()
        {
            try
            {
                Loading = true;
                IsSaving = true;

                if (Settings.SettingsId == 0)
                    await Context.AddAsync(Settings);
                else
                    Context.Update(Settings);

                await Context.SaveChangesAsync();

                AlertOptions.Show = true;
                AlertOptions.AlertType = AlertComponent.AlertTypes.success;
                AlertOptions.Title = "Great!";
                AlertOptions.Message = "Your settings have been saved.";
                AlertOptions.DismissIn = TimeSpan.FromSeconds(3);
            }
            catch (Exception ex)
            {
                AlertOptions.Show = true;
                AlertOptions.AlertType = AlertComponent.AlertTypes.danger;
                AlertOptions.Title = "Oops! We had trouble saving settings";
                AlertOptions.ErrorMessage = ex.ToString();
            }
            finally
            {
                Loading = false;
                IsSaving = false;
            }            
        }        
    }
}
