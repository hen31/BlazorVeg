using Newtonsoft.Json;
using Veg.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Veg.API.Client
{
    public class MembersClient : BaseClient<Member>
    {
        public MembersClient(HttpClient client, ITokenProvider tokenProvider, IAPIClientSettings settings) : base(client, tokenProvider,settings, "members")
        {
        }

        public async Task<Member> ChangeUsernameAsync(string username)
        {

            var response = await (await GetHttpClient().ConfigureAwait(false)).PostAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/changeusername/", username), null).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Member>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var exception = new Exception("Gebruikersnaam bestaat al");
                throw exception;
            }
            ThrowResponseException(response);
            return default;
        }

        public async Task<ICollection<Member>> MoveMemberAndOthersWithSameAdressAsync(Guid memberIdAsGuid, string newAdress, string newZipCode, string newTown)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).PutAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/", memberIdAsGuid.ToString(), "/MoveMemberAndOthersWithSameAdress"), new StringContent(JsonConvert.SerializeObject(new string[] { newAdress, newZipCode, newTown }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<Member>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return default;
        }
        //RoomMates/{memberId}
        public async Task<ICollection<Member>> GetRoomMatesAsync(Guid memberIdAsGuid)
        {
            var response = await (await GetHttpClient().ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/RoomMates/", memberIdAsGuid.ToString())).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ICollection<Member>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return default;
        }
        public async Task<Member> GetMeAsync()
        {
            var response = await(await GetHttpClient().ConfigureAwait(false)).GetAsync(string.Concat(await _settings.GetAPIUrl(), _area, "/me")).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Member>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            ThrowResponseException(response);
            return default;
        }
    }
}
