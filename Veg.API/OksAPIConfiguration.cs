using Microsoft.Extensions.Configuration;
using Veg.API.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.API
{
    public class VegAPIConfiguration
    {
        public VegAPIConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection("VegConfiguration");
            SMTPHost = section.GetValue<string>("SMTPHost", "");
            SMTPUsername = section.GetValue<string>("SMTPUsername", "");
            SMTPPassword = section.GetValue<string>("SMTPPassword", "");
            SMTPSSL = section.GetValue<bool>("SMTPSSL", true);
            SMTPPort = section.GetValue<int>("SMTPPort", 465);
            FromEmailForAll = section.GetValue<string>("FromEmailForAll", "");
            FromEmailForAllDescription = section.GetValue<string>("FromEmailForAllDescription", "");

        }

        public string SMTPHost { get; }
        public string SMTPUsername { get; }
        public string SMTPPassword { get; }
        public bool SMTPSSL { get; }
        public int SMTPPort { get; }
        public string FromEmailForAll { get; }
        public string FromEmailForAllDescription { get; }
    }
}
