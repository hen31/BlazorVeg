using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Entities
{
    public class ReviewSearchResultViewModel
    {

        public int TotalCount { get; set; }
        public List<ProductReview> Reviews { get; set; }
        public int Page { get; set; }
    }
}
