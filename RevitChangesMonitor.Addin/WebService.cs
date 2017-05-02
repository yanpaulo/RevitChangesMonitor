using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RevitChangesMonitor.Addin
{
    public class WebService
    {
        private HttpClient _client;

        public WebService()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri("http://rcm.yanscorp.com/api/")
            };
        }

        public async Task<bool> Authenticate(string userName, string password)
        {
            var credentials = Encoding.ASCII.GetBytes($"{userName}:{password}");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
            if (!(await _client.PostAsync("Authentication", null)).IsSuccessStatusCode)
            {
                _client.DefaultRequestHeaders.Authorization = null;
            }
            return _client.DefaultRequestHeaders.Authorization != null;
        }
    }
}
