using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Veg.SSO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Veg.SSO
{
    public class CustomUserManager : UserManager<ApplicationUser>
    {
        public CustomUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            return base.CreateAsync(user, password);
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            return base.CreateAsync(user);
        }
        public override Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            return base.GetLoginsAsync(user);
        }
    }
}