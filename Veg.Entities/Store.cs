using System;

namespace Veg.Entities
{
    public class Store : BaseEntity
    {
        public string Name { get; set; }

        public bool DefaultInSelection { get; set; }
        public Guid AddedByMemberId { get; set; }
        public virtual Member AddedByMember { get; set; }
        public DateTime DateAdded { get; set; }

        public override string GetDescription()
        {
            return Name;
        }

        public override bool IsValidObject()
        {
            return string.IsNullOrWhiteSpace(Name);
        }
    }
}