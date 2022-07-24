using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Veg.Entities;
using Veg.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.API
{
    public class OnlyMemberAttribute : Attribute, IAsyncActionFilter
    {
        MemberRepository _memberRepository;
        public OnlyMemberAttribute(MemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool authorizedCall = await GetMemberFromContext(context, _memberRepository) != null;
            if (!authorizedCall)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                await next();
            }
        }
        public static async Task<Member> GetLoggedInUser(ActionContext context, MemberRepository _memberRepository)
        {
            if (context != null && context.HttpContext != null)
            {
                var userEmail = context.HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email)?.Value;
                if (userEmail != null)
                {
                    var userVerified = bool.Parse(context.HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.EmailVerified).Value);
                    if (userVerified && !string.IsNullOrWhiteSpace(userEmail))
                    {
                        return await _memberRepository.FindOrCreateMemberWithEmailAdressAsync(userEmail);
                    }
                }
            }
            return null;
        }

        public static async Task<Member> GetMemberFromContext(ActionContext context, MemberRepository _memberRepository)
        {
            Member member = await GetLoggedInUser(context, _memberRepository);
            if (member != null && !member.Disabled)
            {
                return member;
            }
            return null;
        }
    }
}
