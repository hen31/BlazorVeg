using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Entities
{
    public class Report : BaseEntity
    {
        public string Reason { get; set; }
        public string ExtraInformation { get; set; }
        public DateTime AddedAt { get; set; }
        public Guid AddedByMemberId { get; set; }
        public Member AddedByMember { get; set; }
        public bool Handled { get; set; }
        public string HandledMessage { get; set; }
        public Guid? ProductId { get; set; }
        public Product Product { get; set; }
        public Guid? ProductReviewId { get; set; }
        public ProductReview ProductReview { get; set; }

        public override string GetDescription()
        {
            return Reason;
        }

        public override bool IsValidObject()
        {
            return !string.IsNullOrWhiteSpace(Reason) && ((ProductReviewId != null && ProductReviewId != Guid.Empty) || (ProductId != null && ProductId!= Guid.Empty)) ;
        }
    }
}
