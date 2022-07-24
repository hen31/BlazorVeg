using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Veg.Entities;
using Veg.Repositories;
using Veg.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Veg.API
{

    public class OnlyAdminAttribute : Attribute, IAsyncActionFilter
    {
        MemberRepository _memberRepository;
        private readonly bool _siteAdmin;

        public OnlyAdminAttribute(MemberRepository memberRepository, bool siteAdmin)
        {
            _memberRepository = memberRepository;
            _siteAdmin = siteAdmin;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool authorizedCall = await GetMemberFromContext(context);
            if (!authorizedCall)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                await next();
            }
        }


        protected virtual Task<bool> PassesCustomRightsForObjectType(ActionExecutingContext context)
        {
            return Task.FromResult(false);
        }

        protected async Task<Member> GetLoggedInUser(ActionExecutingContext context)
        {
            if (context != null && context.HttpContext != null)
            {
                var userEmail = context.HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value;
                var userVerified = bool.Parse(context.HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.EmailVerified).Value);
                if (userVerified && !string.IsNullOrWhiteSpace(userEmail))
                {
                    return await _memberRepository.FindMemberWithEmailAdressAsync(userEmail);
                }
            }
            return null;
        }

        protected async Task<bool> GetMemberFromContext(ActionExecutingContext context)
        {
            bool authorizedCall = false;

            Member member = await GetLoggedInUser(context);
            if (member != null && !member.Disabled)
            {
                if (member.IsAdmin)
                {
                    authorizedCall = true;
                }
                else if(_siteAdmin == member.IsModerator)
                {
                    authorizedCall = true;
                }
                else if (await PassesCustomRightsForObjectType(context))
                {
                    authorizedCall = true;
                }
            }
            return authorizedCall;
        }
    }

    public class OnlyAdminOrCustomRightCheckAttribute : OnlyAdminAttribute
    {
        private readonly Type reposityType;
        private readonly string idParameter;
        private readonly string objectParameter;

        public OnlyAdminOrCustomRightCheckAttribute(MemberRepository memberRepository, Type reposityType, string IdParameter, string objectParameter, bool siteAdmin) : base(memberRepository, siteAdmin)

        {
            this.reposityType = reposityType;
            idParameter = IdParameter;
            this.objectParameter = objectParameter;
        }

        protected override async Task<bool> PassesCustomRightsForObjectType(ActionExecutingContext context)
        {
            if (!string.IsNullOrWhiteSpace(idParameter) && !string.IsNullOrWhiteSpace(objectParameter))
            {
                return await (context.HttpContext.RequestServices.GetService(reposityType) as EFRepository).CheckIfMemberMayChangeObject((Guid)context.ActionArguments[idParameter], (BaseEntity)context.ActionArguments[objectParameter], await GetLoggedInUser(context));

            }
            else if (!string.IsNullOrWhiteSpace(idParameter))
            {
                return await (context.HttpContext.RequestServices.GetService(reposityType) as EFRepository).CheckIfMemberMayChangeObject((Guid)context.ActionArguments[idParameter], await GetLoggedInUser(context));

            }
            else if (!string.IsNullOrWhiteSpace(objectParameter))
            {
                return await (context.HttpContext.RequestServices.GetService(reposityType) as EFRepository).CheckIfMemberMayChangeObject((BaseEntity)context.ActionArguments[objectParameter], await GetLoggedInUser(context));
            }
            else
            {
                return await base.PassesCustomRightsForObjectType(context);
            }
        }
    }
}
