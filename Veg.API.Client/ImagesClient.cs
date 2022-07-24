using Newtonsoft.Json;
using Veg.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Veg.API.Client
{
    public class ImagesClient : BareClient
    {
        public ImagesClient(HttpClient client, ITokenProvider tokenProvider, IAPIClientSettings settings) : base(client, tokenProvider, settings)
        {
        }
        public async Task<string> UploadImage(byte[] fileData)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), "images"), new StringContent(JsonConvert.SerializeObject(new Image() { Data = Convert.ToBase64String(fileData) }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var uploadImageName =  await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return uploadImageName;
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;
                ThrowResponseException(response);
                return string.Empty;
            }

        }

        public async Task<string> DownloadImage(string imageSize, string imageName)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).GetAsync(string.Concat((await _settings.GetAPIUrl()).Replace("api/",""), @"imagestore/" + imageSize + "/" +  imageName)).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Convert.ToBase64String(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;
                ThrowResponseException(response);
                return string.Empty;
            }

        }

        public async Task UploadImageForMember(Member currentMember, byte[] fileData)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), "images/" + currentMember.ID), new StringContent(JsonConvert.SerializeObject(new Image() { Data = Convert.ToBase64String(fileData) }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;
                ThrowResponseException(response);
            }

        }

        public async Task RemoveImageForMember(Member currentMember)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), "images/remove/" + currentMember.ID), new StringContent("", Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false); ;
                ThrowResponseException(response);
            }

        }
    }
}
