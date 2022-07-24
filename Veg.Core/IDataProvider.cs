using System;

namespace Veg.Core
{
    public interface IDataProvider
    {
        void SetCurrentUser(Guid userId);

    }
}