using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Entities
{
    public class ProductTagLink : BaseEntity
    {
        public Guid TagId { get; set; }
        public Tag Tag { get; set; }

        public override string GetDescription()
        {
            return Tag.Name;
        }

        public override bool IsValidObject()
        {
            return TagId != Guid.Empty;
        }
    }
}
