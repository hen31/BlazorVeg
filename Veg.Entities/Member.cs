using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Veg.Entities
{
    public class Member : BaseEntity
    {
        public string UserName { get; set; }

        public DateTime UserSince { get; set; }

        public bool HasCustomProfileImage { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public bool IsModerator { get; set; }

        [HighDetailProperty]
        [JsonIgnore]
        public string EmailAdress{ get; set; }

        public bool Disabled { get; set; }

        public override string GetDescription()
        {
            return UserName;
        }

        public string GetImagePath()
        {
            if(HasCustomProfileImage)
            {
                return @"imagestore/" + ID.ToString() + ".png";
            }
            else
            {
                return @"imagestore/blank-profile.png";
            }
        }

        public override bool IsValidObject()
        {
            return !string.IsNullOrWhiteSpace(UserName);
        }
    }
}
