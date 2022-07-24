using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Core
{
    public interface IUserProvider
    {
        Guid GetUserId();
        void SetUserId(Guid userId);
        Guid GetCommunityId();
        void SetCommunityId(Guid communityId);

    }
}
