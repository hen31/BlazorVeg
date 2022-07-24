using Newtonsoft.Json;
using Veg.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Veg.API.Client
{
    public abstract class BaseClient<T> : BareClient
    {
        protected string _area;
        protected ITokenProvider _tokenProvider;
        public BaseClient(HttpClient client,ITokenProvider tokenProvider, IAPIClientSettings settings, string area):base(client, tokenProvider, settings)
        {
            _tokenProvider = tokenProvider;
            _area = area;
        }

        public async Task<T> GetItemAsync(Guid id)
        {
            var response = await(await GetHttpClient().ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/", id.ToString())).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return default(T);
        }

        public virtual async Task<ICollection<T>> GetCollectionAsync(FilterPagingOptions filterPagingOptions)
        {
            return await GetItemsAsync(filterPagingOptions);
        }

        public async Task<ICollection<T>> GetItemsAsync(FilterPagingOptions filterPagingOptions, bool withAutherization = true)
        {
            var response = await (await GetHttpClient(withAutherization).ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "?filterpaging=", filterPagingOptions.ToQueryString())).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<T>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return null;
        }

        public async Task<int> GetCountAsync(FilterPagingOptions filterPagingOptions)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/count", "?filterpaging=", filterPagingOptions.ToQueryString())).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return 0;
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).DeleteAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/", id.ToString())).ConfigureAwait(false);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ThrowResponseException(response);
            }
        }

        public async Task<T> AddAsync(T item)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), _area), new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return default(T);
        }
        public async Task<T> EditAsync(Guid id, T item)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PutAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/", id.ToString()), new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return default(T);
        }
    }
}
