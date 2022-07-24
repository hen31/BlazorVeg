using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Blazorise;
using Blazorise.Material;
using Blazorise.Icons.Material;
using Blazorise.Bootstrap;
using Veg.API.Client;
using Veg.App.Pages;
using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using System.Linq;
using Veg.Client.Services;

namespace Veg.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddBlazorise(options =>
                             {
                                 options.ChangeTextOnKeyPress = true;
                             })
                             .AddMaterialProviders()
                             .AddMaterialIcons();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.RootComponents.Add<App>("app");

            ConfigureServices(builder.Services);
            var host = builder.Build();
            host.Services
                .UseBootstrapProviders()
                .UseMaterialProviders()
                .UseMaterialIcons();
            await host.RunAsync();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            foreach (var client in ClientRegistry.GetClientImplementationTypes())
            {
                services.AddScoped(client);
            }

            services.AddScoped<ITokenProvider, LoginRepository>();
            services.AddScoped<ILoginRepository, LoginRepository>();
            services.AddBlazoredLocalStorage();

            //services.AddFileReaderService(options => options.UseWasmSharedBuffer = false);
            if (!services.Any(x => x.ServiceType == typeof(VegConfiguration)))
            {
                services.AddSingleton<VegConfiguration>((s) => new VegConfiguration(s.GetService<HttpClient>(), s.GetService<NavigationManager>()));
            }
            services.AddSingleton<VegImageService>();

            services.AddSingleton<IAPIClientSettings>((s) => s.GetService<VegConfiguration>());
        }
    }
}
