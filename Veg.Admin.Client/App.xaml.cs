using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Veg.API.Client;

namespace Veg.Admin.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static public Container Container { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Create a new Simple Injector container
            Container = new Container();

            // 2. Configure the container (register)
            foreach (var client in ClientRegistry.GetClientImplementationTypes())
            {
                Container.Register(client);
            }
            Container.RegisterSingleton<HttpClient>(() => new HttpClient());

            Container.RegisterSingleton<ITokenProvider, LoginRepository>();
            Container.Register<IAPIClientSettings, ApiConfiguration>();
            Container.RegisterSingleton<LoginRepository>();


            // 3. Verify your configuration
            Container.Verify();
        }

        public class ApiConfiguration : IAPIClientSettings
        {
            public async Task<string> GetAPIUrl()
            {
                return "http://localhost:5003/api/";

            }
        }
    }
}
