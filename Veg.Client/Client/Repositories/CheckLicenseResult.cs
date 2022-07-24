using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veg.App.Pages
{
    public enum CheckLicenseErrorType
    {
        NoConnection,
        ModelError
    }
    public class CheckLicenseResult
    {
        public bool IsError { get;  set; }
        public CheckLicenseErrorType ErrorType { get; set; }
        public bool Succes { get;  set; }
        public DateTime LicenseNowValidTill { get; set; }
    }
}
