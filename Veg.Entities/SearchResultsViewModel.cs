using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Entities
{
    public class SearchResultsViewmodel
    {
        public List<ProductCategory> Categories { get; set; }
        public List<Brand> Brands { get; set; }


        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public List<Product> Products { get; set; }

        public string SortedBy { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
