using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Voupon.Common;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class UpdateIPTableCommand
    {

        public static void Execute(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            Console.WriteLine("Starting UpdateIPTableCommand.Execute");
            StreamReader reader = new StreamReader(File.OpenRead($"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/IP2LOCATION-LITE-DB11.csv"));
            var currentCount = 0;
            var batchInsertCount = 0;
            var newList = new List<IPLookups>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line))
                {
                    //ifc 11 > 7
                    var ipLookups = new IPLookups();
                    string[] values = line.Split(',');
                    if (values.Length > 10)
                    {
                        ipLookups = new IPLookups
                        {
                            Id = Guid.NewGuid(),
                            IPFrom = long.Parse(values[0].Replace("\"", "")),
                            IPTo = long.Parse(values[1].Replace("\"", "")),
                            CountryCode = values[2].Replace("\"", ""),
                            CountryName = values[3].Replace("\"", ""),
                            RegionName = values[4].Replace("\"", ""),
                            CityName = values[5].Replace("\"", "") + " " + values[6].Replace("\"", ""),
                            Latitude = decimal.Parse(values[7].Replace("\"", "")),
                            Longitude = decimal.Parse(values[8].Replace("\"", "")),
                            ZipCode = values[9].Replace("\"", ""),
                            TimeZone = values[10].Replace("\"", ""),
                        };
                    }
                    else
                    {
                        ipLookups = new IPLookups
                        {
                            Id = Guid.NewGuid(),
                            IPFrom = long.Parse(values[0].Replace("\"", "")),
                            IPTo = long.Parse(values[1].Replace("\"", "")),
                            CountryCode = values[2].Replace("\"", ""),
                            CountryName = values[3].Replace("\"", ""),
                            RegionName = values[4].Replace("\"", ""),
                            CityName = values[5].Replace("\"", ""),
                            Latitude = decimal.Parse(values[6].Replace("\"", "")),
                            Longitude = decimal.Parse(values[7].Replace("\"", "")),
                            ZipCode = values[8].Replace("\"", ""),
                            TimeZone = values[9].Replace("\"", ""),
                        };
                    }

                    batchInsertCount++;
                    currentCount++;
                    newList.Add(ipLookups);
                }

                if (batchInsertCount == 300000)
                {
                    Console.WriteLine($"Inserting 300000 records");
                    vodusV2Context.BulkInsert(newList);
                    batchInsertCount = 0;
                    newList.Clear();
                }
                Console.WriteLine($"Processed items : {currentCount}");
            }

            if (newList != null && newList.Any())
            {
                Console.WriteLine($"Inserting remaining {newList.Count()} records");
                vodusV2Context.BulkInsert(newList);
            }

            Console.WriteLine($"===============================================================================");
            Console.WriteLine($"Done processing csv, row count {currentCount}");
            Console.WriteLine("");
            Console.WriteLine("Completed UpdateIPTableCommand.Execute");
            return;
        }
    }
}