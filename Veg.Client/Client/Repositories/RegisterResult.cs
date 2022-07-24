using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Veg.App.Pages
{
    public enum RegisterErrorType
    {
        NoConnection,
        ModelError
    }
    public class RegisterResult
    {
        public bool IsError { get; internal set; }
        public RegisterErrorType ErrorType { get; set; }
        public List<IdentityError> Errors { get; internal set; }
        public bool Succes { get; internal set; }
    }
}
