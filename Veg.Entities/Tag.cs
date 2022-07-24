using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }

        public override string GetDescription()
        {
            return Name;
        }

        public override bool IsValidObject()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}
