using Veg.Entities;
using Veg.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Veg.Repositories
{
    public class MemberRepository : EFRepository<Member>
    {
        public MemberRepository(VegDatabaseContext databaseContext)
        {
            this.DbContext = databaseContext;
        }
        public override DbSet<Member> DbSet => DbContext.Members;

        public override IQueryable<Member> ApplyFiltering(IQueryable<Member> query, Dictionary<string, string> filterValues)
        {
            if (filterValues.Count == 1 && !string.IsNullOrWhiteSpace(filterValues["SingleSearch"]))
            {
                return query.Where(b => EF.Functions.Like(b.UserName, $"%{filterValues["SingleSearch"]}%"));
            }
            return query;
        }

        public override async Task<Member> UpdateAsync(Guid id, Member entity)
        {
            var dbMember = await DbSet.FirstOrDefaultAsync(b => b.ID == id);
            entity.EmailAdress = dbMember.EmailAdress;
            return await base.UpdateAsync(id, entity);
        }
        public async Task<bool> CheckIfUserNameExists(string username)
        {
            return await DbSet.AnyAsync(b => b.UserName == username);
        }

        public override async Task<bool> CheckIfMemberMayChangeObject(Guid guid, BaseEntity baseEntity, Member member)
        {
            if (guid == member.ID)
            {
                Member dbMember = await GetItemByIdAsync(guid);
                Member changedFieldsMember = (Member)baseEntity;
                if (dbMember.IsAdmin != changedFieldsMember.IsAdmin && dbMember.IsModerator != changedFieldsMember.IsModerator)

                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            else
            {
                return false;
            }
        }

        public override void ConfigureModel(EntityTypeBuilder<Member> modelBuilder)
        {
        }

        public async Task SetHasNoImage(Guid memberId)
        {
            var member = await GetItemByIdAsync(memberId);
            member.HasCustomProfileImage = false;
            await UpdateAsync(memberId, member);
        }

        public async Task<Member> ChangeUsernameOfUser(Guid memberId, string username)
        {
            var member = await GetItemByIdAsync(memberId);
            member.UserName = username;
            return await UpdateAsync(memberId, member);
        }

        public async Task<Member> FindMemberWithEmailAdressAsync(string emailAdres)
        {
            return await DbSet.Where(b => EF.Functions.Like(b.EmailAdress, $"%{emailAdres}%")).SingleOrDefaultAsync();
        }

        public async Task<Member> FindOrCreateMemberWithEmailAdressAsync(string emailAdres)
        {
            var userAlreadyExisting = await FindMemberWithEmailAdressAsync(emailAdres);
            if (userAlreadyExisting != null)
            {
                return userAlreadyExisting;
            }
            else
            {
                return await AddAsync(new Member()
                {
                    EmailAdress = emailAdres,
                    IsAdmin = false,
                    IsModerator = false,
                    UserSince = DateTime.Now,
                });
            }
        }


        public async Task SetHasImage(Guid memberId)
        {
            var member = await GetItemByIdAsync(memberId);
            member.HasCustomProfileImage = true;
            await UpdateAsync(memberId, member);
        }

       
    }
}
