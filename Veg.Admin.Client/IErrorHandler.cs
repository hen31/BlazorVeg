using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Admin.Client
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
