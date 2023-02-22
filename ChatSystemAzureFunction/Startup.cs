using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Database.Postgres.RewardsEntities;
using ChatSystemAzureFunction.Services.Blob;
using Voupon.Common.Azure.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Features;


[assembly: FunctionsStartup(typeof(ChatSystemAzureFunction.Startup))]

namespace ChatSystemAzureFunction
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services.AddDbContext<VodusV2Context>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("SqlConnectionString")));
            builder.Services.AddDbContext<RewardsDBContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("RewardsSqlConnectionString")));
            builder.Services.AddSingleton<IAzureBlob, AzureBlob>();
            builder.Services.AddSingleton<IAzureBlobStorage, AzureBlobStorage>();
            builder.Services.AddSingleton<IAzureBlobStorage>(factory =>
            {
                return new AzureBlobStorage(new AzureBlobSettings(
                    storageAccount: Environment.GetEnvironmentVariable("AzureConfigurationsStorageAccount"),
                    storageKey: Environment.GetEnvironmentVariable("AzureConfigurationsStorageKey")));
            });
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MemoryBufferThreshold = Int32.MaxValue;
            });
        }
    }
}