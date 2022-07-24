using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.SSO.Models
{
    public class License
    {
        public long ID { get; set; }
        public ApplicationUser User { get; set; }
        public string Application { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Key { get; set; }
    }
}
