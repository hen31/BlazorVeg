using System;
using System.Collections.Generic;

namespace Veg.Entities
{
    public class ProductReview : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; }

        public string Content { get; set; }

        public virtual ICollection<ReviewImage> ReviewImages { get; set; }

        public byte Rating { get; set; }
        public virtual Product Product { get; set; }
        public Guid ProductId { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateOfMutation { get; set; }

        public override string GetDescription()
        {
            return Content;
        }

        public override bool IsValidObject()
        {
            return true;
        }
    }
}