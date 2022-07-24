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
    [Authorize]
    [TypeFilter(typeof(OnlyMemberAttribute))]
    public class ReportsController : BaseController<ReportRepository, Report>
    {
        private MemberRepository _memberRepository;

        public ReportsController(ReportRepository repository, MemberRepository memberRepository)
            : base(repository)
        {
            _memberRepository = memberRepository;
        }

        [TypeFilter(typeof(OnlyMemberAttribute))]
        public override async Task<ActionResult<Report>> AddAsync([FromBody] Report entity)
        {
            var userEmail = HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value;
            var loggedInMember = await (_memberRepository as MemberRepository).FindMemberWithEmailAdressAsync(userEmail);
            entity.AddedByMemberId = loggedInMember.ID;
            entity.AddedAt = DateTime.UtcNow;
            entity.ProductReview = null;
            entity.Handled = false;
            entity.Product = null;
            if(!entity.IsValidObject())
            {
                return BadRequest();
            }
            return await base.AddAsync(entity);
        }

        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false })]
        public override async Task<ActionResult<Report>> GetItemByIdAsync(Guid id)
        {
            return  await base.GetItemByIdAsync(id);
        }


        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false})]
        public override Task<ActionResult<bool>> DeleteAsync([FromBody] Report entity)
        {
            return base.DeleteAsync(entity);
        }

        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false})]
        public override Task<ActionResult> DeleteItemByIdAsync(Guid id)
        {
            return base.DeleteItemByIdAsync(id);
        }


        [TypeFilter(typeof(OnlyAdminAttribute), Arguments = new object[] { false})]
        public override async Task<ActionResult<Report>> UpdateAsync(Guid id, [FromBody]Report entity)
        {
            return await base.UpdateAsync(id, entity);
        }
    }
}
