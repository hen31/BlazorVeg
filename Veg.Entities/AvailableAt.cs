using System;
using System.Security.Cryptography.X509Certificates;

namespace Veg.Entities
{
    public class AvailableAt : BaseEntity
    {
        public Guid StoreId { get; set; }
        public Store Store { get; set; }
        public DateTime DateAdded { get; set; }
        public virtual Member AddedByMember { get; set; }
        public Guid AddedByMemberId { get; set; }

        public override string GetDescription()
        {
            return "";
        }

        public override bool IsValidObject()
        {
            return StoreId != Guid.Empty;
        }
    }
}