using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
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
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Voupon.Common.Azure.Blob;
using Voupon.Common.SMS.SMSS360;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Services.Base.Queries.Single;
using Voupon.Rewards.WebApp.Services.Identity.Commands;

namespace Voupon.Rewards.WebApp
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
            services.AddMediatR(typeof(BasePageQuery).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(SendResetLinkCommand).GetTypeInfo().Assembly);

            var _appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            //  var sendgridSection = Configuration.GetSection("Sendgrid");
            //  services.Configure<Sendgrid>(Configuration.GetSection("Sendgrid"));
            services.AddTransient<UserManager<Users>>();
            services.AddTransient<RoleManager<Roles>>();
            services.AddTransient<SignInManager<Users>>();

            services.AddAutoMapper(typeof(Startup));

            //  AddRazorRuntimeCompilation frontend page refresh without recompiling
            //services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddDbContext<VodusV2Context>(options => options.UseNpgsql(_appSettings.Database.VodusConnectionString));
            services.AddDbContext<Voupon.Database.Postgres.RewardsEntities.RewardsDBContext>(options => options.UseNpgsql(_appSettings.Database.RewardsConnectionString));

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(_appSettings.Cache.RedisConnectionString));

            services.AddSingleton<ITagHelperInitializer<ScriptTagHelper>, AppendVersionTagHelperInitializer>();
            services.AddSingleton<ITagHelperInitializer<LinkTagHelper>, AppendVersionTagHelperInitializer>();

            services.AddIdentity<Users, Roles>(options =>
                {
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequiredUniqueChars = 0;
                } )
            .AddEntityFrameworkStores<VodusV2Context>()
            .AddDefaultTokenProviders();

            services.AddAuthorization();
            services.AddControllersWithViews();

            services.AddScoped<IAzureBlobStorage>(factory => new AzureBlobStorage(new AzureBlobSettings(
                storageAccount: Configuration.GetSection("AppSettings").GetSection("AzureConfigurations")["StorageAccount"],
                storageKey: Configuration.GetSection("AppSettings").GetSection("AzureConfigurations")["StorageKey"])));

            //For SMS Integration
            services.AddScoped<ISMSS360>(factory => new SMSS360(email: Configuration.GetSection("AppSettings").GetSection("SMSS360")["Email"],
                apiKey: Configuration.GetSection("AppSettings").GetSection("SMSS360")["APIKey"]));

             services.ConfigureApplicationCookie(options =>
                    {
                        options.Cookie.HttpOnly = true;
                        options.ExpireTimeSpan = TimeSpan.FromDays(36500);
                        options.LoginPath = "/relogin";
                        options.AccessDeniedPath = "/error/access-denied";
                        options.SlidingExpiration = true;
                        options.Cookie.SameSite = SameSiteMode.Lax;
                        //options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
                        //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        
                    });
        }
        
       

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            string baseDir = env.ContentRootPath;
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(baseDir, "App_Data"));

#if DEBUG
            app.UseHttpsRedirection();
#endif
            app.UseStaticFiles();
            app.UseCors();
            app.UseRouting();
            //app.UseMvc();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
