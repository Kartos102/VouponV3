using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using Voupon.Common.Azure.Blob;
using Voupon.ConsoleApp.Services;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        public static IConfigurationRoot _configuration;
        private static VodusV2Context _vodusV2Context;
        private static RewardsDBContext _rewardsDBContext;
        private static IAzureBlobStorage _azureBlobStorage;

        static void Main()
        {
            Main2(_vodusV2Context);
        }

        private async static void Main2(VodusV2Context vodusV2Context)
        {
            //_vodusV2Context = vodusV2Context;
            //  Setup
            RegisterServices();

            //Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("Voupon Tools v1.0.4-r1");

            //Console.WriteLine("--------------------------------------------------------------------");
            //Console.WriteLine("\n-> Which server to connect to?\nlive/uat/local");
            //var environment = Console.ReadLine().ToLower();

            _vodusV2Context = _serviceProvider.GetService<VodusV2Context>();
            _rewardsDBContext = _serviceProvider.GetService<RewardsDBContext>();
            _azureBlobStorage = _serviceProvider.GetRequiredService<IAzureBlobStorage>();

            Start:
            Console.WriteLine("Connecting to LIVE");
            //Console.WriteLine("\n--------------------------------------------------------------------");
            Console.WriteLine("\n-> Choose your action (Enter the number of your choice and press enter)");
            Console.WriteLine("1 = Regenerate Google Merchant XML");
            Console.WriteLine("200 = Delete unused memberprofile");


            int actionType = 0;
            if (!int.TryParse(Console.ReadLine(), out actionType))
            {
                Console.WriteLine("Invalid selection. Enter \"1\" to download data or \"2\" to upload data");
                goto End;
            }

            if (actionType == 1)
            {
                //RegenerateGoogleMerchantXMLCommand.Execute(_azureBlobStorage, _vodusV2Context, _rewardsDBContext).Wait();
                //CreatePsyFromResponsesCommand.Execute(_vodusV2Context, _rewardsDBContext);
                EndCommercialForMemberProfileCommand.Execute(_vodusV2Context, _rewardsDBContext);
            }

            else if (actionType == 200)
            {
                Console.WriteLine("Enter hours. followed by duration eg: 0-10 (12am run for 10 hours, 0-24)");
                var hoursInput = Console.ReadLine().ToLower();

                var hoursInputArray = hoursInput.Split("-");
                if(hoursInputArray.Length != 2)
                {
                    Console.WriteLine("Invalid hours");
                    goto End;
                }

                int hours = 0;
                int duration = 0;
                if (!int.TryParse(hoursInputArray[0], out hours))
                {
                    Console.WriteLine("Invalid hours format");
                    goto End;
                }

                if (!int.TryParse(hoursInputArray[1], out duration))
                {
                    Console.WriteLine("Invalid duration format");
                    goto End;
                }
                    
                Console.WriteLine("Enter sort type, asc or desc");
                var sortType = Console.ReadLine().ToLower();

                if(sortType != "asc" && sortType != "desc")
                {
                    Console.WriteLine("Invalid selection");
                    goto End;
                }

                DeleteNotInUseMemberProfileCommand.Execute(_vodusV2Context, sortType, hours, duration).Wait();
                //CreatePsyFromResponsesCommand.Execute(_vodusV2Context, _rewardsDBContext);
            }
            else
            {
                Console.WriteLine("\nInvalid selection. Enter \"1\" to download data or \"2\" to upload data");
            }

            End:
            Console.WriteLine("\nPress enter to restart the process");

            var next = Console.ReadLine();
            Console.WriteLine("\n--------------------------------------------------------------------");
            Console.WriteLine("\nNew request");
            Console.WriteLine("\n--------------------------------------------------------------------");
            goto Start;
        }

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();

            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();


            collection.AddDbContext<VodusV2Context>(options => options.UseSqlServer(_configuration.GetConnectionString("VodusSqlServerConnection")));
            collection.AddDbContext<RewardsDBContext>(options => options.UseSqlServer(_configuration.GetConnectionString("SqlServerConnection")));

            collection.AddScoped<IAzureBlobStorage>(factory =>
            {
                return new AzureBlobStorage(new AzureBlobSettings(
                    storageAccount: _configuration.GetSection("AppSettings:App:AzureConfigurationStorageAccount").Value,
                    storageKey: _configuration.GetSection("AppSettings:App:AzureConfigurationStorageKey").Value));
            });

            _serviceProvider = collection.BuildServiceProvider();
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
