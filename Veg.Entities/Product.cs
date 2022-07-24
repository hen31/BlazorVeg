using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace Veg.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }

        public Guid BrandId { get; set; }

        public Brand Brand { get; set; }

        [NotMapped]
        public double Rating { get; set; }

        public Guid CategoryId { get; set; } = Guid.Empty;

        public ProductCategory Category { get; set; }


        public ICollection<AvailableAt> StoresAvailable { get; set; }

        public bool IsVegan { get; set; }

        public bool IsVegetarian { get; set; }

        public bool ImageVerified { get; set; }

        public string ProductImage { get; set; }

        public Guid AddedByMemberId { get; set; }
        public Member AddedByMember { get; set; }

        public Guid? BarcodeAddedById { get; set; }
        public Member BarcodeAddedBy { get; set; }

        public Guid? ImageAddedById { get; set; }
        public Member ImageAddedBy { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductReview> Reviews { get; set; }
        public DateTime DateAdded { get; set; }

        public ICollection<ProductTagLink> Tags { get; set; }

        public string Barcode { get; set; }

        public override string GetDescription()
        {
            return Name;
        }

        public override bool IsValidObject()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Brand.Name)
                && (IsVegan ^ IsVegetarian);//throw new NotImplementedException();
        }
    }
}
