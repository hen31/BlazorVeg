using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Veg.Entities
{
    public class LimitResultOptions
    {
        public ICollection<Guid> ProductCategories { get; set; } = new List<Guid>();
        public ICollection<Guid> Brands { get; set; } = new List<Guid>();
        public ICollection<Guid> Tags { get; set; } = new List<Guid>();

        public string ToQueryString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string Sorting { get; set; }
        public bool OnlyWithoutImage { get; set; }
        public bool OnlyNotVerified { get; set; }
    }
}
