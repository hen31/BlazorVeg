using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Veg.Entities
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public ProductCategory ParentCategory { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductCategory> SubCategories { get; set; }
        public string FullCategoryPath
        {
            get
            {
                return ParentCategory != null ? string.Concat(ParentCategory.FullCategoryPath, " > ", Name) : Name;
            }
        }

        public override string GetDescription()
        {
            return Name;
        }

        public override bool IsValidObject()
        {
            return string.IsNullOrEmpty(Name);
        }
    }
}
