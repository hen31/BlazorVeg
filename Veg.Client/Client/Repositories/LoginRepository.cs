//using Veg.API.Client;
//using Veg.Data;
//using Veg.SSO.Controllers;
using Blazored.LocalStorage;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Veg.API.Client;
using Veg.Client;
using Veg.Client.Repositories;
//using Microsoft.AspNetCore.Blazor.Browser.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Veg.App.Pages
{
    public class LoginRepository : ITokenProvider, ILoginRepository
    {
        public IJSRuntime JsRuntime { get; set; }

        public async Task<bool> CheckIfMeExists(User user)
        {
            var httpClient = new HttpClient();
            if (user != null)
            {
                httpClient.SetBearerToken(user.AccessToken);
            }
            var response = (await httpClient.GetAsync(string.Concat(_configuration.APIUrl, "members/me")).ConfigureAwait(false));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
        public async virtual Task<User> GetCurrentUser()
        {

            var base64EncodedBytes = System.Convert.FromBase64String(await JsRuntime.InvokeAsync<string>("getCookie", new object[] { "authCookieVeg" }));
            var json = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);
            // var user = await _protectedLocalStorage.GetItemAsync<User>("CurrentUser");
            //      var lastTime = await _protectedLocalStorage.GetItemAsync<DateTime>("LatestGet");
            if (user == null)
            {
                return null;
            }
            else
            {
                if (Math.Abs((DateTime.Now - user.LastCheckOfLoggedIn).TotalMinutes) > 20)
                {
                    user = await CheckLoginIfNeeded(user);
                    return user;
                }
                else
                {
                    return user;
                }
            }
        }

        private async Task<User> CheckLoginIfNeeded(User user)
        {
            if (await CheckIfMeExists(user))
            {
                await RefreshToken(user);
                user.LastCheckOfLoggedIn = DateTime.Now;
                await SetCurrentUserInCookie(user);
                return user;
            }
            await Logout();
            return null;
        }

        private readonly ILocalStorageService _protectedLocalStorage;
        VegConfiguration _configuration;
        private readonly HttpClient client;

        public LoginRepository(ILocalStorageService protectedLocalStorage, VegConfiguration configuration, HttpClient client, IJSRuntime jSRuntime = null)
        {
            _configuration = configuration;
            this.client = client;
            _protectedLocalStorage = protectedLocalStorage;
            JsRuntime = jSRuntime;
        }
        public async Task<LoginResult> Login(string emailAdress, string password, MembersClient membersClient)
        {

            if (!_configuration.IsLoaded)
            {
                await _configuration.LoadVegConfiguration();
            }
            LoginResult result = new LoginResult();
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration.SSOUrl,
                Policy = { RequireHttps = false }
            });
            if (discoveryResponse.IsError)
            {
                Console.WriteLine(discoveryResponse.Error);
                result.IsError = true;
                result.ErrorType = LoginErrorType.NoConnection;
            }
            else
            {
                //"Veg.API"
                PasswordTokenRequest passwordTokenRequest = new PasswordTokenRequest()
                {
                    Address = discoveryResponse.TokenEndpoint,
                    ClientId = "Veg.Desktop",
                    ClientSecret = "F8TSFbwZuhYB7JSKYA3kgrCpTpU9Fq2t",
                    UserName = emailAdress,
                    Password = password,
                    Scope = "Veg.API openid Veg.SSO offline_access",
                };
                var tokenResponse = await client.RequestPasswordTokenAsync(passwordTokenRequest).ConfigureAwait(false);
                if (tokenResponse.IsError)
                {
                    result.IsError = true;
                    if (tokenResponse.ErrorType == ResponseErrorType.Http)
                    {
                        result.ErrorType = LoginErrorType.NoConnection;
                    }
                    else
                    {
                        result.ErrorType = LoginErrorType.WrongCredentials;
                    }
                }
                else
                {
                    UserInfoRequest infoRequest = new UserInfoRequest()
                    {
                        Address = discoveryResponse.UserInfoEndpoint,
                        Token = tokenResponse.AccessToken
                    };
                    var token = tokenResponse.AccessToken;
                    var userResponse = await client.GetUserInfoAsync(infoRequest).ConfigureAwait(false);
                    if (userResponse.IsError)
                    {
                        Console.WriteLine(userResponse.Error);
                        if (userResponse.ErrorType == ResponseErrorType.Http)
                        {
                            result.ErrorType = LoginErrorType.NoConnection;
                        }
                        else
                        {
                            result.ErrorType = LoginErrorType.NoIdentity;
                        }
                    }
                    else
                    {
                        var claims = userResponse.Claims;
                        result.AccessToken = tokenResponse.AccessToken;
                        bool isAdmin = false;
                        bool isMemberAdministrator = false;
                        bool isModeratorAdministrator = false;
                        string username = string.Empty;
                        Guid memberId = Guid.Empty;
                        await SetCurrentUser(userResponse, tokenResponse.AccessToken, tokenResponse.RefreshToken, isAdmin, username, isModeratorAdministrator, memberId);
                        try
                        {
                            var member = (await membersClient.GetMeAsync());
                            isAdmin = member.IsAdmin;
                            isMemberAdministrator = member.IsAdmin;
                            isModeratorAdministrator = member.IsModerator;
                            username = member.UserName;
                            memberId = member.ID;
                        }
                        catch
                        {
                            await ClearCurrentUser();
                            result.IsError = true;
                            result.ErrorType = LoginErrorType.NoMember;
                            return result;

                        }
                        await SetCurrentUser(userResponse, tokenResponse.AccessToken, tokenResponse.RefreshToken, isAdmin, username, isModeratorAdministrator, memberId);

                    }
                }
            }
            return result;
        }
        public async Task<bool> RefreshToken(User currentUser)
        {
            if (string.IsNullOrWhiteSpace(currentUser.RefreshToken))
            {
                return false;
            }
            if (!_configuration.IsLoaded)
            {
                await _configuration.LoadVegConfiguration();
            }
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration.SSOUrl,
                Policy = { RequireHttps = false }
            });
            if (discoveryResponse.IsError)
            {
                Console.WriteLine(discoveryResponse.Error);
                return false;
            }
            else
            {
                //"Veg.API"
                RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest()
                {
                    Address = discoveryResponse.TokenEndpoint,
                    ClientId = "Veg.Desktop",
                    ClientSecret = "F8TSFbwZuhYB7JSKYA3kgrCpTpU9Fq2t",
                    RefreshToken = currentUser.RefreshToken
                };
                var tokenResponse = await client.RequestRefreshTokenAsync(refreshTokenRequest).ConfigureAwait(false);
                if (tokenResponse.IsError)
                {
                    return false;
                }
                else
                {
                    currentUser.AccessToken = tokenResponse.AccessToken;
                    currentUser.RefreshToken = tokenResponse.RefreshToken;
                    return true;
                }
            }
        }

        public async Task<ConfirmEmailResult> ResetPasswordAsync(string emailAdress, string resetCode, string password)
        {
            if (!_configuration.IsLoaded)
            {
                await _configuration.LoadVegConfiguration();
            }
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration.SSOUrl,
                Policy = { RequireHttps = false }
            });
            if (discoveryResponse.IsError)
            {
                return new ConfirmEmailResult() { IsError = false, ErrorType = ConfirmEmailErrorType.NoConnection };
            }
            else
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new ResetPasswordWithCodeViewModel() { Email = emailAdress, ResetCode = resetCode, Password = password });
                var result = await client.PostAsync(discoveryResponse.Issuer + "/api/Accounts/ResetPasswordWithCode", new StringContent(json, Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return new ConfirmEmailResult() { IsError = false, Succes = true };
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ConfirmEmailResult() { IsError = true, ErrorType = ConfirmEmailErrorType.NoConnection };
                    }
                    else
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var errors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IdentityError>>(content);
                        return new ConfirmEmailResult() { IsError = true, Errors = errors, ErrorType = ConfirmEmailErrorType.ModelError };
                    }
                }
            }
        }

        public async Task<bool> Logout()
        {
           return await ClearCurrentUser();
        }

        public async Task<RegisterResult> Register(string emailAdress, string password)
        {
            if (!_configuration.IsLoaded)
            {
                await _configuration.LoadVegConfiguration();
            }
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration.SSOUrl,
                Policy = { RequireHttps = false }
            });
            if (discoveryResponse.IsError)
            {
                string errorMessage = discoveryResponse.Error;
                return new RegisterResult() { IsError = true, ErrorType = RegisterErrorType.NoConnection };
            }
            else
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new RegistrationViewModel() { Email = emailAdress, Password = password });
                var result = await client.PostAsync(discoveryResponse.Issuer + "/api/Accounts", new StringContent(json, Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return new RegisterResult() { IsError = false, Succes = true };
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new RegisterResult() { IsError = true, ErrorType = RegisterErrorType.NoConnection };
                    }
                    else
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var errors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IdentityError>>(content);
                        return new RegisterResult() { IsError = true, Errors = errors, ErrorType = RegisterErrorType.ModelError };
                    }
                }
            }
        }

        public async Task<ConfirmEmailResult> ConfirmEmailAdress(string emailAdress, string code)
        {
            if (!_configuration.IsLoaded)
            {
                await _configuration.LoadVegConfiguration();
            }
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration.SSOUrl,
                Policy = { RequireHttps = false }
            });
            if (discoveryResponse.IsError)
            {
                return new ConfirmEmailResult() { IsError = false, ErrorType = ConfirmEmailErrorType.NoConnection };
            }
            else
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new ConfirmEmailViewModel() { Email = emailAdress, Code = code });
                var result = await client.PostAsync(discoveryResponse.Issuer + "/api/Accounts/ConfirmEmail", new StringContent(json, Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return new ConfirmEmailResult() { IsError = false, Succes = true };
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ConfirmEmailResult() { IsError = true, ErrorType = ConfirmEmailErrorType.NoConnection };
                    }
                    else
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var errors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IdentityError>>(content);
                        return new ConfirmEmailResult() { IsError = true, Errors = errors, ErrorType = ConfirmEmailErrorType.ModelError };
                    }
                }
            }
        }
        //SendResetCode

        public async Task<ConfirmEmailResult> SendPasswordResetCode(string emailAdress, bool changePassword)
        {
            if (!_configuration.IsLoaded)
            {
                await _configuration.LoadVegConfiguration();
            }
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration.SSOUrl,
                Policy = { RequireHttps = false }
            });
            if (discoveryResponse.IsError)
            {
                return new ConfirmEmailResult() { IsError = false, ErrorType = ConfirmEmailErrorType.NoConnection };
            }
            else
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new ResetPasswordViewModel() { Email = emailAdress, ChangePassword = changePassword });
                var result = await client.PostAsync(discoveryResponse.Issuer + "/api/Accounts/SendResetCode", new StringContent(json, Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return new ConfirmEmailResult() { IsError = false, Succes = true };
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new ConfirmEmailResult() { IsError = true, ErrorType = ConfirmEmailErrorType.NoConnection };
                    }
                    else
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var errors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IdentityError>>(content);
                        return new ConfirmEmailResult() { IsError = true, Errors = errors, ErrorType = ConfirmEmailErrorType.ModelError };
                    }
                }
            }
        }

        public async Task<CheckLicenseResult> CheckLicense(string licenseKey)
        {
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _configuration.SSOUrl,
                Policy = { RequireHttps = false }
            });
            if (discoveryResponse.IsError)
            {
                return new CheckLicenseResult() { IsError = true, ErrorType = CheckLicenseErrorType.NoConnection };
            }
            else
            {
                //client.SetBearerToken(ServiceResolver.GetService<ITokenProvider>().GetBearerToken());
                client.Timeout = new TimeSpan(0, 5, 0);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new LicenseKey() { Key = licenseKey, Application = "Veg" });
                var result = await client.PostAsync(discoveryResponse.Issuer + "/api/LicenseKeys", new StringContent(json, Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var checkLicenseResult = Newtonsoft.Json.JsonConvert.DeserializeObject<CheckLicenseResult>(content);
                    return checkLicenseResult;
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new CheckLicenseResult() { IsError = true, ErrorType = CheckLicenseErrorType.NoConnection };
                    }
                    else
                    {
                        return new CheckLicenseResult() { IsError = true, ErrorType = CheckLicenseErrorType.ModelError };
                    }
                }
            }
        }

        private async Task SetCurrentUser(UserInfoResponse userResponse, string accesToken, string refreshToken, bool isAdmin, string username, bool isSiteAdmin, Guid memberId)
        {
            User user = new User();
            user.EmailAdress = userResponse.Claims.Where(b => b.Type == JwtClaimTypes.Email).SingleOrDefault().Value;
            user.ID = Guid.Parse(userResponse.Claims.Where(b => b.Type == JwtClaimTypes.Subject).SingleOrDefault().Value);
            //user.Claims = userResponse.Claims;
            user.RefreshToken = refreshToken;
            user.LastCheckOfLoggedIn = DateTime.Now;
            user.AccessToken = accesToken;
            user.IsAdmin = isAdmin;
            user.UserName = username;
            user.IsSiteAdministrator = isSiteAdmin;
            user.MemberId = memberId;
            //  await _protectedLocalStorage.SetItemAsync("CurrentUser", user);
            await SetCurrentUserInCookie(user);
        }

        private async Task SetCurrentUserInCookie(User user)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            await JsRuntime.InvokeAsync<object>("WriteCookie", new object[] { "authCookieVeg", System.Convert.ToBase64String(plainTextBytes), 365 });
        }

        public async Task UsernameChanged(string userName)
        {
            var currentUser = await GetCurrentUser();
            currentUser.UserName = userName;
            await SetCurrentUserInCookie(currentUser);
        }

        private async Task<bool> ClearCurrentUser()
        {
            await JsRuntime.InvokeAsync<object>("WriteCookie", new object[] { "authCookieVeg",  "", 365 });
            await JsRuntime.InvokeAsync<object>("RemoveCookie", new object[] { "authCookieVeg" });
            return true;

        }

        public async Task<string> GetTokenAsync()
        {
            return (await GetCurrentUser().ConfigureAwait(false)).AccessToken;
        }

        public async Task<bool> GetHasTokenAsync()
        {
            var user = await GetCurrentUser().ConfigureAwait(false);
            if (user != null)
            {
                var token = user.AccessToken;
                return !string.IsNullOrWhiteSpace(token);
            }
            return false;
        }


    }

    //
    // Summary:
    //     Encapsulates an error from the identity subsystem.
    public class IdentityError
    {

        //
        // Summary:
        //     Gets or sets the code for this error.
        public string Code { get; set; }
        //
        // Summary:
        //     Gets or sets the description for this error.
        public string Description { get; set; }
    }
}
