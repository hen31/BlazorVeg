using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Veg.API.Client
{
    public interface IAPIClientSettings
    {
        Task<string> GetAPIUrl();
    }
}
