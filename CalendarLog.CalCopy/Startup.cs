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

namespace CalendarLog.CalCopy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
            services.AddScoped<ICopyCalendarService, CopyCalendarService>();

            services.AddHttpClient<ICalendarLogClient, CalendarLogClient>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
            //Electron.Menu.SetApplicationMenu(new MenuItem[] { });

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
}
