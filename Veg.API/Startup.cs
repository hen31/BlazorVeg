using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Veg.API.Client;
using Veg.API.Middleware;
using Veg.Core;
using Veg.Data.EntityFramework.SQL;
using Veg.Repositories;
using Veg.Storage;

namespace Veg.API
{
    public class Startup
    {
        public static string WebRootPath { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.AddAuthorization();
            services.AddAuthentication("Bearer").AddIdentityServerAuthentication(options =>
            {
#if DEBUG
                options.Authority = "http://localhost:5010";
#else
                options.Authority = "https://sso.todo.com";
#endif
                options.RequireHttpsMetadata = false;
                options.EnableCaching = true;
                options.CacheDuration = TimeSpan.FromMinutes(10); // that's the default
                options.ApiName = "Veg.API";
            });
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton(new VegAPIConfiguration(Configuration));
            services.AddLogging((config => config.AddConsole().AddDebug()));
            services.AddCors(options =>
            options.AddPolicy("AllowAllOrigins", builder1 => builder1.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            services.AddScoped<VegDatabaseContext>((services) => new SQLContext(Configuration.GetConnectionString("DefaultConnection")));
            foreach (Type repositoryType in VegRepositoriesRegistration.GetRespositoryImplementationTypes())
            {
                services.AddScoped(repositoryType);
            }
            new SQLContext(Configuration.GetConnectionString("DefaultConnection"));




    }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            WebRootPath = env.WebRootPath;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder =>
          builder.AllowAnyOrigin().AllowAnyHeader().WithMethods("POST","GET","UPDATE","DELETE")
                 );


            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            //app.UseProtectImageStore();

            app.UseStaticFiles();

            //app.UseHttpsRedirection();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
