using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Veg.Core
{
    public class ServiceResolver
    {
        private Container _container;
        static private ServiceResolver _instance;

        public static Container GetContainer()
        {
            if (GetInstance()._container == null)
            {
                GetInstance()._container = new Container();
            }
            return GetInstance()._container;
        }

        public static T GetDataProvider<T>() where T : class, IDataProvider
        {
           // var userProvider = GetService<IUserProvider>();
            var dataProvider = GetContainer().GetInstance<T>();
            //var userId = userProvider.GetUserId();
           // dataProvider.SetCurrentUser(userId);
            return dataProvider;
        }

        public static void ResetContainer()
        {
            _instance = new ServiceResolver();
        }

        private static ServiceResolver GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ServiceResolver();
            }
            return _instance;
        }

        public static T GetService<T>() where T : class
        {
            return GetContainer().GetInstance<T>();
        }
    }
}
