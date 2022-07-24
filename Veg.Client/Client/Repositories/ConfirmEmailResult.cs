using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veg.App.Pages
{
    public enum ConfirmEmailErrorType
    {
        NoConnection,
        ModelError
    }
    public class ConfirmEmailResult
    {
        public bool IsError { get; internal set; }
        public ConfirmEmailErrorType ErrorType { get; set; }
        public List<IdentityError> Errors { get; internal set; }
        public bool Succes { get; internal set; }
    }
}
