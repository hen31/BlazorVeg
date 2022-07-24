using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Veg.Client;

namespace Veg.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VegConfigurationController : ControllerBase
    {
        private IConfiguration configuration;

        public VegConfigurationController(IConfiguration iConfig)
        {
            configuration = iConfig;
        }
        [HttpGet("")]
        public string GetVegConfiguration()
        {
            VegConfiguration VegConfiguration = VegConfiguration.FromConfig(configuration);
            // string json = "{\r\n \"APIUrl\": \"http://hen311-002-site5.atempurl.com/api/\", \"SSOUrl\": \"http://hen311-002-site4.atempurl.com\", \"ChurchName\": \"Fonteinkerk\", \"Slogan\": \"Gereformeerde kerk vrijgemaakt\", \"ShowServicesInMenu\": true, \"ShowChurchName\": false, \"ShowChurchLogo\": true, \"SloganInTitle\": true, \"FrontPage\": {\r\n \"ShowHeaderImage\": true, \"ShowServices\": true,\r\n \"MaxNumberOfServices\": 8, \"AudioService\": \"LiveURLOnly\", \"LiveAudioServiceUrl\": \"http://fonteinkerk.kerkdienstluisteren.nl/\"\r\n } } ";
            string json = JsonConvert.SerializeObject(VegConfiguration);
            return json;
        }

    }
}