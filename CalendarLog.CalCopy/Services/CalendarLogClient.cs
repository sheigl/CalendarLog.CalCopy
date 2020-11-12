using CalendarLog.CalCopy.Models;
using CalendarLog.CalCopy.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public class CalendarLogClient : ICalendarLogClient
    {
        private readonly HttpClient _client;
        private readonly CalCopyDbContext _context;

        public CalendarLogClient(HttpClient client,
            CalCopyDbContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<List<CalendarEntryVM>> GetCalendarsByApiKeyAsync()
        {
            Settings settings = await _context.Settings.FirstOrDefaultAsync();

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, new Uri($"{settings.APIUrl}?apiKey={settings.APIKey}"));
            req.Content = new StringContent(JsonConvert.SerializeObject(new ApiKeyVM { SecretKey = settings.SecretKey, APIKey = settings.APIKey }), Encoding.UTF8, "application/json");

            HttpResponseMessage res = await _client.SendAsync(req);
            res.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<List<CalendarEntryVM>>(await res.Content.ReadAsStringAsync());
        }
    }
}
