using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.SSO.Controllers
{
    public class ResetPasswordWithCodeViewModel
    {
        public string Email { get; set; }
        public string ResetCode { get; set; }
        public string Password { get; set; }
    }
}
