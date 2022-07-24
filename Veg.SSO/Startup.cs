using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veg.SSO.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veg.SSO.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Veg.SSO.Services;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Services;
using IdentityServer4;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Microsoft.Extensions.Logging;
using Serilog;
using IdentityServer4.EntityFramework.Entities;

namespace Veg.SSO
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("token", options =>
                 {
                     options.Authority = "https://sso.todo.com/";
                     options.RequireHttpsMetadata = false;
                     options.EnableCaching = true;
                     options.CacheDuration = TimeSpan.FromMinutes(10); // that's the default
                     options.ApiName = "Veg.SSO";
                 });
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Tokens.EmailConfirmationTokenProvider = "Email";
                options.Tokens.PasswordResetTokenProvider = "Phone";
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = new TimeSpan(0, 5, 0);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedEmail = true;
            }).AddErrorDescriber<CustomIdentityErrorDescriber>()
                  .AddEntityFrameworkStores<ApplicationDbContext>()
                  .AddDefaultTokenProviders()
                  .AddDefaultUI()
                  .AddUserManager<CustomUserManager>();

            // Add application services.
            services.AddSingleton<IProfileService, ProfileService<ApplicationUser>>();
            if (File.Exists(Path.Combine(".", "IdentityServer4Auth.pfx")))
            {
                Log.Logger.Information("File extsis");
            }
            else
            {
                Log.Logger.Information("File not extsis");

            }
            services.AddMvc(mvcOptions => mvcOptions.EnableEndpointRouting = false);
            var certificate = new X509Certificate2(Path.Combine(".", "IdentityServer4Auth.pfx"), "TODO", X509KeyStorageFlags.MachineKeySet);
            services.AddCors(options =>
              options.AddPolicy("AllowAllOrigins", builder1 => builder1.WithOrigins("http://localhost:50456", "https://localhost:50456",
              "https://localhost:5000", "http://localhost:5000", "http://hen311-002-site6.atempurl.com", "http://hen311-002-site5.atempurl.com",
              "https://sso.todo.com", "https://api.todo.com", "https://todo.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
            services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "/Identity/Account/Login";
                options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
            })

                    .AddSigningCredential(certificate)
                    .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseSqlServer(connectionString,
                                sql => sql.MigrationsAssembly(migrationsAssembly));
                    })
                     // this adds the operational data from DB (codes, tokens, consents)
                     .AddOperationalStore(options =>
                     {
                         options.ConfigureDbContext = builder =>
                             builder.UseSqlServer(connectionString,
                                 sql => sql.MigrationsAssembly(migrationsAssembly));
                         // this enables automatic token cleanup. this is optional.
                         options.EnableTokenCleanup = true;
                         options.TokenCleanupInterval = 3600;
                     })
                      .AddAspNetIdentity<ApplicationUser>()
                      .AddProfileService<CustomProfileService>();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // this will do the initial DB population
            InitializeDatabase(app);

            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
            app.UseAuthentication();
            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors("AllowAllOrigins");
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.EnsureCreated();

                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                var appContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                appContext.Database.Migrate();

                while (context.Clients.FirstOrDefault() != null)
                {
                    context.Remove(context.Clients.FirstOrDefault());
                    context.SaveChanges();
                }

                foreach (var client in Config.GetClients())
                {
                    if (context.Clients.Where(b => b.ClientId == client.ClientId).Count() == 0)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                }
                context.SaveChanges();


                while (context.IdentityResources.FirstOrDefault() != null)
                {
                    context.Remove(context.IdentityResources.FirstOrDefault());
                    context.SaveChanges();
                }
                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                while (context.ApiResources.FirstOrDefault() != null)
                {
                    context.Remove(context.ApiResources.FirstOrDefault());
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }


            }
        }
    }
}
