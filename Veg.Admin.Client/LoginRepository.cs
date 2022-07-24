//using Veg.API.Client;
//using Veg.Data;
//using Veg.SSO.Controllers;
using IdentityModel;
using IdentityModel.Client;
using Veg.API.Client;
//using Microsoft.AspNetCore.Blazor.Browser.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Veg.Admin.Client
{
    public class LoginRepository : ITokenProvider
    {
        public static User CurrentUser { get; set; }

        private readonly HttpClient client;

        public LoginRepository(HttpClient client)
        {
            this.client = client;
        }

        public async Task<LoginResult> Login(string emailAdress, string password, MembersClient membersClient)
        {

            LoginResult result = new LoginResult();
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
#if DEBUG
                Address = "http://localhost:5010",
#else
                Address = "https://sso.todo.com",
#endif
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
                        SetCurrentUser(userResponse, tokenResponse.AccessToken, tokenResponse.RefreshToken, isAdmin, username, isModeratorAdministrator, memberId);
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
                            result.IsError = true;
                            result.ErrorType = LoginErrorType.NoMember;
                            return result;

                        }
                        SetCurrentUser(userResponse, tokenResponse.AccessToken, tokenResponse.RefreshToken, isAdmin, username, isModeratorAdministrator, memberId);

                    }
                }
            }
            return result;
        }
        private void SetCurrentUser(UserInfoResponse userResponse, string accesToken, string refreshToken, bool isAdmin, string username, bool isSiteAdmin, Guid memberId)
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
            CurrentUser = user;
        }

        public async Task<string> GetTokenAsync()
        {
            return CurrentUser.AccessToken;
        }

        public async Task<bool> GetHasTokenAsync()
        {
            var user = CurrentUser;
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
