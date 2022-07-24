using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Veg.Entities
{
    public abstract class BaseEntity
    {
        public Guid ID { get; set; }

        public abstract bool IsValidObject();
        public abstract string GetDescription();
    }

}
