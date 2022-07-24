using System;
using System.Text.Json.Serialization;

namespace Veg.Entities
{
    public class ReviewImage : BaseEntity
    {
        public Guid ReviewID { get; set; }
        [JsonIgnore]
        public virtual ProductReview Review { get; set; }
        public string ImageName { get; set; }

        public override string GetDescription()
        {
            return ImageName;
        }

        public override bool IsValidObject()
        {
            return true;
        }
    }
}