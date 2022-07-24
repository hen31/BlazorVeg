using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veg.Classes
{
    public enum CheckLicenseErrorType
    {
        NoConnection,
        ModelError
    }
    public class CheckLicenseResult
    {
        public bool IsError { get; internal set; }
        public CheckLicenseErrorType ErrorType { get; set; }
        public bool Succes { get; internal set; }
        public DateTime LicenseNowValidTill { get; set; }
    }
}
