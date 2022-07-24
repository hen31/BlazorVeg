using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Veg.API.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Veg.Client
{
    public class VegConfiguration : IAPIClientSettings
    {
        [JsonIgnore]
        public HttpClient HttpClient { get; set; }
        [JsonIgnore]
        public NavigationManager NavigationManager { get; set; }
        public VegConfiguration()
        {

        }
        public static VegConfiguration FromConfig(IConfiguration configuration)
        {
            VegConfiguration config = new VegConfiguration();
            var section = configuration.GetSection("VegConfiguration");
            config.APIUrl = section["APIUrl"];
            config.SSOUrl = section["SSOUrl"];
#if DEBUG
            config.APIUrl = "http://localhost:5003/api/";
            config.SSOUrl = "http://localhost:5010";
#endif
            config.SiteName = section["SiteName"];
            config.IsLoaded = true;
            return config;
        }
        public VegConfiguration(HttpClient HttpClient, NavigationManager NavigationManager)
        {
            this.HttpClient = HttpClient;
            this.NavigationManager = NavigationManager;

        }
        public async Task LoadVegConfiguration()
        {
            if (NavigationManager != null)
            {
                string json = await HttpClient.GetStringAsync(NavigationManager.BaseUri + "api\\VegConfiguration");
                var config = JsonConvert.DeserializeObject<VegConfiguration>(json);
                SiteName = config.SiteName;
                APIUrl = config.APIUrl;
                SSOUrl = config.SSOUrl;
                IsLoaded = true;
            }
        }

        public async Task<string> GetAPIUrl()
        {
            if (!IsLoaded)
            {
                await LoadVegConfiguration();
            }
            return APIUrl;
        }

        public string GetAPIBasePath()
        {
            return APIUrl.Remove(APIUrl.Length - "api/".Length);
        }

        public bool ShowChurchName { get; set; } = true;
        public bool ShowServicesInMenu { get; set; } = true;
        public bool ShowChurchLogo { get; set; } = false;
        public bool SloganInTitle { get; set; } = false;

        public bool InvertLogoAndName { get; set; } = false;
        public string SiteName { get; set; } = "";
        public string APIUrl { get; set; } = "";
        public string SSOUrl { get; set; } = "";
        public string Slogan { get; set; } = "";
        public string Description { get; set; } = "";
        public FrontPageSettings FrontPageSettings { get; set; } = new FrontPageSettings();
       [JsonIgnore]
        public bool IsLoaded { get; internal set; } = false;
    }




    public enum AudioServiceType { None, LiveURLOnly };

    public class FrontPageSettings
    {
        public AudioServiceType GetAudioService()
        {
            if (AudioService.Equals("LiveURLOnly", StringComparison.OrdinalIgnoreCase))
            {
                return AudioServiceType.LiveURLOnly;
            }
            else
            {
                return AudioServiceType.None;
            }
        }
        public FrontPageSettings()
        {

        }
        public FrontPageSettings(IConfigurationSection configurationSection)
        {
            ShowHeaderImage = configurationSection.GetValue<bool>("ShowHeaderImage", true);
            ShowServices = configurationSection.GetValue<bool>("ShowServices", false);
            AudioService = configurationSection.GetValue<string>("AudioService", "None");
            LiveAudioServiceUrl = configurationSection.GetValue<string>("LiveAudioServiceUrl", "None");
            ServicesMaxDaysInFuture = configurationSection.GetValue<int>("ServicesMaxDaysInFuture", 21);
        }
        public bool ShowHeaderImage { get; set; } = true;
        public bool ShowServices { get; set; } = false;
        public string AudioService { get; set; } = "None";
        public string LiveAudioServiceUrl { get; set; } = "None";
        public int ServicesMaxDaysInFuture { get; set; } = 21;
    }
}
