using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Veg.Entities;

namespace Veg.API.Client
{
    public class ReportsClient : BaseClient<Report>
    {
        public ReportsClient(HttpClient client, ITokenProvider tokenProvider, IAPIClientSettings settings) : base(client, tokenProvider, settings, "reports")
        {
            ForceTokenUsage = true;
        }


    }
}
