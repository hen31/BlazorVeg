using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Veg.App.Pages
{
    public class User
    {
        public bool IsAdmin { get; set; }
        public string EmailAdress { get; set; }
        public DateTime LastCheckOfLoggedIn { get; set; }
        public Guid ID { get; set; }
        public string AccessToken
        {
            get; set;
        }
        public string RefreshToken
        { get; set; }
        public Guid MemberId { get; set; }
        public bool IsSiteAdministrator { get; set; }
        public string UserName { get; set; }
    }
}
