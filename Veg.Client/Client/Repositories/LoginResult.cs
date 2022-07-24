using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veg.App.Pages
{
    public enum LoginErrorType { NoConnection, WrongCredentials, NoIdentity,
        NoMember
    }
    public class LoginResult
    {
        public LoginErrorType ErrorType { get; internal set; }
        public bool IsError { get; internal set; }
        public string AccessToken { get; internal set; }
        // internal User User { get; set; }
    }
}
