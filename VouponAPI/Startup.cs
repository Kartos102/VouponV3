using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Newtonsoft.Json;
using Voupon.API.Util;
using Voupon.Common.SMS.SMSS360;

[assembly: FunctionsStartup(typeof(Voupon.API.Startup))]

namespace Voupon.API
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services.AddDbContext<VodusV2Context>(options => options.UseNpgsql(Environment.GetEnvironmentVariable(EnvironmentKey.SQL_CONNECTION_STRING)));
            builder.Services.AddDbContext<RewardsDBContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable(EnvironmentKey.SQL_REWARDS_CONNECTION_STRING)));

            //  Example of addung enable retry
            //  builder.Services.AddDbContext<VodusV2Context>(options => options.UseSqlServer(SqlConnection, providerOptions => providerOptions.EnableRetryOnFailure()));


            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(EnvironmentKey.REDIS_CONNECTION_STRING)));

            //For SMS Integration
            builder.Services.AddScoped<ISMSS360>(factory =>
            {
                return new SMSS360(email: Environment.GetEnvironmentVariable(EnvironmentKey.SMS.SMS_EMAIL),
                    apiKey: Environment.GetEnvironmentVariable(EnvironmentKey.SMS.SMS_API_KEY));
            });


            builder.Services.AddScoped<IAzureBlobStorage>(factory =>
            {
                return new AzureBlobStorage(new AzureBlobSettings(
                    storageAccount: Environment.GetEnvironmentVariable(EnvironmentKey.AZURECONFIGURATION.STORAGE_ACCOUNT),
                    storageKey: Environment.GetEnvironmentVariable(EnvironmentKey.AZURECONFIGURATION.STORAGE_KEY)));
            });

            builder.Services.AddIdentity<Database.Postgres.VodusEntities.Users, Database.Postgres.VodusEntities.Roles>()
             .AddEntityFrameworkStores<VodusV2Context>()
             .AddDefaultTokenProviders();


            builder.Services.AddTransient<UserManager<Database.Postgres.VodusEntities.Users>>();
            builder.Services.AddTransient<RoleManager<Database.Postgres.VodusEntities.Roles>>();
            builder.Services.AddHttpClient();
            ThreadPool.SetMinThreads(1000, 1000);

        }
    }
}