using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Veg.SSO
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
                    {
                        new ApiResource("Veg.API", "Veg API"),
                        new ApiResource("Veg.SSO", "Veg SSO"),
                    };
        }


        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
                   {
                       new Client
                       {
                           ClientId = "Veg.Desktop",
                           AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                           RequireConsent = false,
                           AccessTokenLifetime = 36000,
                           RefreshTokenUsage = TokenUsage.ReUse,
                           RefreshTokenExpiration = TokenExpiration.Sliding,
                           AlwaysIncludeUserClaimsInIdToken = true,
                           AllowedCorsOrigins=new List<string>(){ "http://localhost:50456", "https://localhost:50456",
                           "https://localhost:5000", "http://localhost:5000", "http://hen311-002-site6.atempurl.com", "http://hen311-002-site5.atempurl.com",
                           "https://sso.todo.com", "https://api.todo.com", "https://todo.com"},
                           
                           // secret for authentication
                           ClientSecrets =
                           {
                               new Secret("F8TSFbwZuhYB7JSKYA3kgrCpTpU9Fq2t".Sha256())
                           },
                           // scopes that client has access to
                           AllowedScopes = {
                                IdentityServerConstants.StandardScopes.OpenId,
                                IdentityServerConstants.StandardScopes.Profile,
                                IdentityServerConstants.StandardScopes.Email,
                                IdentityServerConstants.StandardScopes.OfflineAccess,
                                "Veg.API",
                                "Veg.SSO"},

                           AllowOfflineAccess = true
                       },
                         new Client
                       {
                           ClientId = "Veg.App",
                           AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                           RequireConsent = false,
                           AccessTokenLifetime = 36000,
                           RefreshTokenUsage = TokenUsage.ReUse,
                           RefreshTokenExpiration = TokenExpiration.Sliding,
                           AlwaysIncludeUserClaimsInIdToken = true,
                           AllowedCorsOrigins=new List<string>(){ "http://localhost:50456", "https://localhost:50456",
              "https://localhost:5000", "http://localhost:5000", "http://hen311-002-site6.atempurl.com", "http://hen311-002-site5.atempurl.com", 
                               "https://todo.com", "https://sso.todo.com", "https://api.todo.com"},
                           // secret for authentication
                           ClientSecrets =
                           {
                               new Secret("F8TSFbwZuhYB7JSKYA3kgrCpTpU9Fq2t".Sha256())
                           },
                           // scopes that client has access to
                           AllowedScopes = {
                                IdentityServerConstants.StandardScopes.OpenId,
                                IdentityServerConstants.StandardScopes.Profile,
                                IdentityServerConstants.StandardScopes.Email,
                                IdentityServerConstants.StandardScopes.OfflineAccess,
                                "Veg.API",
                                "Veg.SSO"},

                           AllowOfflineAccess = true
                       }
                   };
        }

        internal static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
                                            {
                                                new IdentityResources.OpenId(),
                                                new IdentityResources.Profile(),
                                                new IdentityResources.Email()
                                            };
        }
    }
}
