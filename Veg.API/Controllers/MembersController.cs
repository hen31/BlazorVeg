using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Veg.Core;
using Veg.Entities;
using Veg.Repositories;
using Veg.Storage;

namespace Veg.API.Controllers
{

    public class MembersController : BaseController<MemberRepository, Member>
    {

        public MembersController(MemberRepository repository)
            : base(repository)
        {

        }
        [Authorize]
        [TypeFilter(typeof(OnlyMemberAttribute))]
        public override Task<ActionResult<ICollection<Member>>> GetCollectionAsync([FromQuery(Name = "filterpaging")] string filterPagingOptionsJson)
        {
            return base.GetCollectionAsync(filterPagingOptionsJson);
        }
        [Authorize]
        [TypeFilter(typeof(OnlyMemberAttribute))]
        public override Task<ActionResult<int>> GetCountAsync([FromQuery(Name = "filterpaging")] string filterPagingOptionsJson)
        {
            return base.GetCountAsync(filterPagingOptionsJson);
        }

        [TypeFilter(typeof(OnlyMemberAttribute))]
        [HttpGet("me")]
        public virtual async Task<ActionResult<Member>> GetMeAsync()
        {
            return Ok(await (_repository as MemberRepository).FindOrCreateMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value));
        }
        [TypeFilter(typeof(OnlyMemberAttribute))]
        [Authorize]
        [HttpPost("changeusername/{username}")]
        public async Task<ActionResult<Member>> ChangeUsernameOfUser(string username)
        {
            if (await (_repository as MemberRepository).CheckIfUserNameExists(username))
            {
                return BadRequest("Error01");
            }
            else if (string.IsNullOrEmpty(username) || username.Length < 5 || username.Length > 25)
            {
                return BadRequest("Error02");
            }
            else
            {
                var userEmail = HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value;
                var loggedInMember = await (_repository as MemberRepository).FindMemberWithEmailAdressAsync(userEmail);
                return Ok(await (_repository as MemberRepository).ChangeUsernameOfUser(loggedInMember.ID, username));
            }
        }

        [Authorize]
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        public override async Task<ActionResult<Member>> AddAsync([FromBody] Member entity)
        {
            var userEmail = HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value;
            var loggedInMember = await (_repository as MemberRepository).FindMemberWithEmailAdressAsync(userEmail);
            if (!loggedInMember.IsAdmin)
            {
                entity.IsAdmin = false;
                entity.IsModerator = false;
            }
            return await base.AddAsync(entity);
        }



        [Authorize]
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        public override Task<ActionResult<bool>> DeleteAsync([FromBody] Member entity)
        {
            if (entity.IsAdmin)
            {
                this.Unauthorized();
            }
            return base.DeleteAsync(entity);
        }

        [Authorize]
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        public override Task<ActionResult> DeleteItemByIdAsync(Guid id)
        {
            return base.DeleteItemByIdAsync(id);
        }


        [Authorize]
        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { true })]
        public override async Task<ActionResult<Member>> UpdateAsync(Guid id, [FromBody] Member entity)
        {
            var userEmail = HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value;
            var loggedInMember = await (_repository as MemberRepository).FindMemberWithEmailAdressAsync(userEmail);
            var dbUser = await (_repository as MemberRepository).GetItemByIdAsync(id);
            entity.EmailAdress = dbUser.EmailAdress;

            if (!loggedInMember.IsAdmin)
            {
                entity.IsAdmin = dbUser.IsAdmin;
                entity.IsModerator = dbUser.IsModerator;
            }

            return await base.UpdateAsync(id, entity);
        }

        [HttpGet("{id}")]
        public override async Task<ActionResult<Member>> GetItemByIdAsync(Guid id)
        {
            if (!HttpContext.User.Claims.ToList().Any(r => r.Type == JwtClaimTypes.Email))
            {
                var memberFromDb = await _repository.GetItemByIdAsync(id);
                return Ok(memberFromDb);
            }
            var userEmail = HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value;
            var loggedInMember = await (_repository as MemberRepository).FindMemberWithEmailAdressAsync(userEmail);
            if (id != loggedInMember?.ID)
            {
                var memberFromDb = await _repository.GetItemByIdAsync(id);
                return Ok(memberFromDb);
            }
            else
            {
                return await base.GetItemByIdAsync(id);
            }
        }

        protected override async Task<ICollection<Member>> InternalGetCollectionAsync(string filterPagingOptionsJson)
        {
            var collection = await base.InternalGetCollectionAsync(filterPagingOptionsJson);
            return collection.ToList();
        }
    }
}
