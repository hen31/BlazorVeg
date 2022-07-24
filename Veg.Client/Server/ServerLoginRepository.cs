using Blazored.LocalStorage;
using Microsoft.AspNetCore.Http;
using Veg.App.Pages;
using Veg.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Veg.Server
{
    public class ServerLoginRepository : LoginRepository
    {
        public ServerLoginRepository(VegConfiguration configuration, HttpClient client, IHttpContextAccessor httpContextAccessor)
            : base(null, configuration, client)
        {
            HttpContextAccessor = httpContextAccessor;
        }
       static User _user = null;

        public IHttpContextAccessor HttpContextAccessor { get; }

        public void SetUser(HttpRequest request)
        {
          
        }

        public override async Task<User> GetCurrentUser()
        {
            /*if (HttpContextAccessor.HttpContext.Request.Headers.TryGetValue("Cookie", out Microsoft.Extensions.Primitives.StringValues values))
            {
                var cookies = values.ToString().Split(';').ToList();
                var result = cookies.Select(c => new { Key = c.Split('=')[0].Trim(), Value = c.Split('=')[1].Trim() }).ToList();
                var userJson = result.Where(r => r.Key == "authCookieVeg").FirstOrDefault();
                if (userJson != null)
                {
                    var base64EncodedBytes = System.Convert.FromBase64String(userJson.Value);
                    var json = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                    _user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);
                }
            }*/

            if(HttpContextAccessor.HttpContext.Request.Cookies["authCookieVeg"] != null)
            {
                var base64EncodedBytes = System.Convert.FromBase64String(HttpContextAccessor.HttpContext.Request.Cookies["authCookieVeg"]);
                var json = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);
            }
            return null;
        }
    }
}