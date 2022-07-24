using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json.Serialization;

namespace Veg.Entities
{
    public class Brand : BaseEntity
    {
        public string BrandImage { get; set; }
        public string Name { get; set; }

        public Guid AddedByMemberId { get; set; }
        public virtual Member AddedByMember { get; set; }

        [JsonIgnore]
        [HighDetailProperty]
        public virtual ICollection<Product> Products { get; set; }
        public DateTime DateAdded { get; set; }

        public override string GetDescription()
        {
            return Name;
        }

        public override bool IsValidObject()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}