using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Voupon.Common.Azure.Blob;
using Voupon.Common.SMS.SMSS360;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.App.Services.Queries.List;

namespace Voupon.Merchant.WebApp
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
            services.AddMediatR(typeof(ListBanksQuery).GetTypeInfo().Assembly);

            var _appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddTransient<UserManager<Users>>();
            services.AddTransient<RoleManager<Roles>>();
            services.AddTransient<SignInManager<Users>>();

            services.AddDbContext<RewardsDBContext>(options => options.UseNpgsql(_appSettings.Database.RewardsConnectionString));
            services.AddDbContext<Voupon.Database.Postgres.VodusEntities.VodusV2Context>(options => options.UseNpgsql(_appSettings.Database.VodusConnectionString));
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(_appSettings.Cache.RedisConnectionString));

            services.AddIdentity<Users, Roles>()
            .AddEntityFrameworkStores<RewardsDBContext>()
            .AddDefaultTokenProviders();

            //  AddRazorRuntimeCompilation frontend page refresh without recompiling
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+*";
                options.User.RequireUniqueEmail = true;

                options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(3);
                options.ClaimsIssuer = "Voupon";
                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/access-denied";              
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddScoped<IAzureBlobStorage>(factory =>
            {
                return new AzureBlobStorage(new AzureBlobSettings(
                    storageAccount: Configuration.GetSection("AppSettings").GetSection("AzureConfigurations")["StorageAccount"],
                    storageKey: Configuration.GetSection("AppSettings").GetSection("AzureConfigurations")["StorageKey"]));
            });
            //
            services.AddScoped<ISMSS360>(factory =>
            {
                return new SMSS360(email: Configuration.GetSection("AppSettings").GetSection("SMSS360")["Email"],
                    apiKey: Configuration.GetSection("AppSettings").GetSection("SMSS360")["APIKey"]);
            });


            services.AddHttpContextAccessor();
            services.AddScoped<IPrincipal>(
                (sp) => sp.GetService<IHttpContextAccessor>().HttpContext.User
            );

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseDeveloperExceptionPage();
#if DEBUG
            app.UseHttpsRedirection();
#endif
            app.UseStaticFiles();
            app.UseCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                endpoints.MapAreaControllerRoute(
                    name: "area",
                    areaName: "app",
                    pattern: "app/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapAreaControllerRoute(
                    name: "area",
                    areaName: "admin",
                    pattern: "admin/{controller=Home}/{action=Index}/{id?}");
            });
            Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath,"Rotativa");
            //  Uncomment this to create roles/users during startup
            //var result = CreateRoles(serviceProvider).Result;
        }


        private async Task<bool> CreateRoles(IServiceProvider serviceProvider)
        {
            //adding custom roles
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Roles>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Users>>();

            string[] roleNames = { "Manager", "Supervisor", "Staff" };

            foreach (var roleName in roleNames)
            {
                //creating the roles and seeding them to the database
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    var roleResult = await roleManager.CreateAsync(new Roles { Id = Guid.NewGuid(), Name = roleName, NormalizedName = roleName.Normalize() });
                }
            }

            //creating a super user who could maintain the web app
            var poweruser = new Users()
            {
                Id = Guid.NewGuid(),
                UserName = Configuration.GetSection("SetupUserSettings")["Username"],
                Email = Configuration.GetSection("SetupUserSettings")["Email"],
                NormalizedEmail = Configuration.GetSection("SetupUserSettings")["Email"].Normalize(),
                NormalizedUserName = Configuration.GetSection("SetupUserSettings")["Username"].Normalize(),
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.Now
            };


            string userPassword = Configuration.GetSection("SetupUserSettings")["Password"];
            var _user = await userManager.FindByEmailAsync(Configuration.GetSection("SetupUserSettings")["Email"]);

            if (_user == null)
            {
                var cre = await userManager.CreateAsync(poweruser, userPassword);
                if (cre.Succeeded)
                {
                    await userManager.AddToRoleAsync(poweruser, "Merchant");
                }
            }
            return true;
        }
    }
}
