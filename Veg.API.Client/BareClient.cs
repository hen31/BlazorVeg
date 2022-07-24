using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Veg.API.Client
{
    public abstract class BareClient : IClient
    {
        ITokenProvider _tokenProvider;
        HttpClient _httpClient;
        protected bool ForceTokenUsage { get; set; } = false;
        protected IAPIClientSettings _settings;
        public BareClient(HttpClient httpClient, ITokenProvider tokenProvider, IAPIClientSettings settings)
        {
            _tokenProvider = tokenProvider;
            _httpClient = httpClient;
            _settings = settings;
        }

        public async Task<HttpClient> GetHttpClient(bool useToken = true)
        {
            //_httpClient.BaseAddress = new Uri(await _settings.GetAPIUrl());
            if ((useToken || ForceTokenUsage) && await _tokenProvider.GetHasTokenAsync())
            {
                _httpClient.SetBearerToken(await _tokenProvider.GetTokenAsync());
            }
            else
            {
                _httpClient.SetBearerToken("");
            }
            return _httpClient;

        }

        protected void ThrowResponseException(HttpResponseMessage response)
        {
            ExceptionOnApiCall?.Invoke(response);
        }

        public static Action<HttpResponseMessage> ExceptionOnApiCall { get; set; }

    }
}
