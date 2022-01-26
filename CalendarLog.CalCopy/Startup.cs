using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.EntityFrameworkCore;
using CalendarLog.CalCopy.Models;
using CalendarLog.CalCopy.Services;
using System.IO;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace CalendarLog.CalCopy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "calcopy.db"
                );

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDbContext<CalCopyDbContext>(opts => opts.UseSqlite($"Filename={dbPath}"));
            services.AddSingleton<IElectronService, ElectronService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddScoped<ICopyCalendarService, CopyCalendarService>()
                .AddScoped<GoogleSheetsClient>();

            //services.AddHttpClient<ICalendarLogClient, CalendarLogClient>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CalCopyDbContext context)
        {
            context.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });

            BootStrap();
        }

        public async void BootStrap()
        {
            if (HybridSupport.IsElectronActive)
            {
                MenuInit();

                BrowserWindowOptions options = new BrowserWindowOptions
                {
                    Show = false,
                    DarkTheme = true,
                    //TitleBarStyle = TitleBarStyle.hiddenInset,
                    BackgroundColor = "#343d45"
                };

                BrowserWindow window = await Electron.WindowManager.CreateWindowAsync(options);
                window.WebContents.OnCrashed += async (killed) =>
                {
                    var options = new MessageBoxOptions("This process has crashed.")
                    {
                        Type = MessageBoxType.info,
                        Title = "Renderer Process Crashed",
                        Buttons = new string[] { "Reload", "Close" }
                    };
                    var result = await Electron.Dialog.ShowMessageBoxAsync(options);

                    if (result.Response == 0)
                    {
                        window.Reload();
                    }
                    else
                    {
                        window.Close();
                    }
                };

                window.OnUnresponsive += async () =>
                {
                    var options = new MessageBoxOptions("This process is hanging.")
                    {
                        Type = MessageBoxType.info,
                        Title = "Renderer Process Hanging",
                        Buttons = new string[] { "Reload", "Close" }
                    };

                    var result = await Electron.Dialog.ShowMessageBoxAsync(options);

                    if (result.Response == 0)
                    {
                        window.Reload();
                    }
                    else
                    {
                        window.Close();
                    }
                };

                window.OnReadyToShow += () => window.Show();
            }
        }

        private void MenuInit()
        {
            if (HybridSupport.IsElectronActive)
            {
                var menu = new MenuItem[] 
                {
                    new MenuItem
                    { 
                        Label = "File",
                        Submenu = new MenuItem[]
                        {
                            new MenuItem { Label = "Quit", Accelerator = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Cmd+Q" : "Alt+F4", Role = MenuRole.quit }
                        }
                    },
                    new MenuItem 
                    { 
                        Label = "Edit", 
                        Submenu = new MenuItem[] 
                        {
                            new MenuItem { Label = "Undo", Accelerator = "CmdOrCtrl+Z", Role = MenuRole.undo },
                            new MenuItem { Label = "Redo", Accelerator = "Shift+CmdOrCtrl+Z", Role = MenuRole.redo },
                            new MenuItem { Type = MenuType.separator },
                            new MenuItem { Label = "Cut", Accelerator = "CmdOrCtrl+X", Role = MenuRole.cut },
                            new MenuItem { Label = "Copy", Accelerator = "CmdOrCtrl+C", Role = MenuRole.copy },
                            new MenuItem { Label = "Paste", Accelerator = "CmdOrCtrl+V", Role = MenuRole.paste },
                            new MenuItem { Label = "Select All", Accelerator = "CmdOrCtrl+A", Role = MenuRole.selectall }
                        }
                    },
                    new MenuItem 
                    { 
                        Label = "Window", 
                        Role = MenuRole.window, 
                        Submenu = new MenuItem[] 
                        {
                            new MenuItem { Label = "Minimize", Accelerator = "CmdOrCtrl+M", Role = MenuRole.minimize },
                            new MenuItem { Label = "Close", Accelerator = "CmdOrCtrl+W", Role = MenuRole.close }
                        }
                    }
                };

                Electron.Menu.SetApplicationMenu(menu);
            }
        }
    }
}
