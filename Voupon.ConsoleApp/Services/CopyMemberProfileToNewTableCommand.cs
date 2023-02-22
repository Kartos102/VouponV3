using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Voupon.Common;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class CopyMemberProfileToNewTableCommand
    {

        public static void Execute(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            Console.WriteLine("Starting CopyMemberProfileToNewTableCommand.Execute");

            var stopProcessing = false;
            var currentCount = 0;

            while (!stopProcessing)
            {
                //var row = vodusV2Context.Database.ExecuteSqlRaw("Select top 1 * from newmemberprofiles");

                long nextId = 0;
                var startTime = DateTime.Now;
                using (var command = vodusV2Context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT TOP 1 * from MemberProfiles order by id desc";
                    command.CommandType = CommandType.Text;
                    //var parameter = new SqlParameter("@p1", "");
                    //command.Parameters.Add(parameter);

                    vodusV2Context.Database.OpenConnection();

                    using (var result = command.ExecuteReader())
                    {
                        if (result.HasRows)
                        {
                            while (result.Read())
                            {
                                nextId = long.Parse(result["Id"].ToString());
                            }
                        }

                    }

                    var rowsInserted = vodusV2Context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT MemberProfiles ON;INSERT INTO MemberProfiles([Id] ,	[Guid] ,	[Email] ,	[DateOfBirth] ,	[AddressLine1] ,	[AddressLine2] ,	[AddressLine3] ,	[City] ,	[Postcode] ,	[StateId] ,	[CountryId] ,	[DistrictId] ,	[AreaId] ,	[CreatedAt] ,	[CreatedAtPartnerWebsiteId] ,	[CreatedAtCountryCode] ,	[MasterMemberProfileId],	[IsMasterProfile] ,	[AvailablePoints] ,	[LastRespondedAt],	[DemographicAgeId],	[DemographicEducationId] ,	[DemographicGenderId],	[DemographicOccupationId] ,	[DemographicEthnicityId],	[DemographicReligionId] ,	[DemographicMonthlyIncomeId] ,	[DemographicMaritalStatusId],	[DemographicStateId] ,	[DemographicPoints] ,	[DemographicLastUpdatedAt] ,	[LastCommercialRequestedAt] ,	[DemographicAgeIdPartnerWebsiteId] ,	[DemographicEducationPartnerWebsiteId],	[DemographicGenderPartnerWebsiteId],	[DemographicOccupationPartnerWebsiteId] ,	[DemographicEthnicityPartnerWebsiteId],	[DemographicReligionPartnerWebsiteId],	[DemographicMonthlyIncomePartnerWebsiteId],	[DemographicMaritalStatusPartnerWebsiteId],	[DemographicStatePartnerWebsiteId],	[DemographicRuralUrbanId],	[DemographicRuralUrbanPartnerWebsiteId],	[ResponseCount] ,	[CloseClickCount] ,	[CloseClickLastUpdatedAt] ,	[DemographicMonthlyHouseHoldIncomeId] ,	[DemographicMonthlyHouseHoldIncomePartnerWebsiteId],	[MemberR] ,	[SyncMemberProfileId] ,	[SyncMemberProfileToken])SELECT top 500000 [Id] ,	[Guid] ,	[Email] ,	[DateOfBirth] ,	[AddressLine1] ,	[AddressLine2] ,	[AddressLine3] ,	[City] ,	[Postcode] ,	[StateId] ,	[CountryId] ,	[DistrictId] ,	[AreaId] ,	[CreatedAt] ,	[CreatedAtPartnerWebsiteId] ,	[CreatedAtCountryCode] ,	[MasterMemberProfileId],	[IsMasterProfile] ,	[AvailablePoints] ,	[LastRespondedAt],	[DemographicAgeId],	[DemographicEducationId] ,	[DemographicGenderId],	[DemographicOccupationId] ,	[DemographicEthnicityId],	[DemographicReligionId] ,	[DemographicMonthlyIncomeId] ,	[DemographicMaritalStatusId],	[DemographicStateId] ,	[DemographicPoints] ,	[DemographicLastUpdatedAt] ,	[LastCommercialRequestedAt] ,	[DemographicAgeIdPartnerWebsiteId] ,	[DemographicEducationPartnerWebsiteId],	[DemographicGenderPartnerWebsiteId],	[DemographicOccupationPartnerWebsiteId] ,	[DemographicEthnicityPartnerWebsiteId],	[DemographicReligionPartnerWebsiteId],	[DemographicMonthlyIncomePartnerWebsiteId],	[DemographicMaritalStatusPartnerWebsiteId],	[DemographicStatePartnerWebsiteId],	[DemographicRuralUrbanId],	[DemographicRuralUrbanPartnerWebsiteId],	[ResponseCount] ,	[CloseClickCount] ,	[CloseClickLastUpdatedAt] ,	[DemographicMonthlyHouseHoldIncomeId] ,	[DemographicMonthlyHouseHoldIncomePartnerWebsiteId],	[MemberR] ,	[SyncMemberProfileId] ,	[SyncMemberProfileToken] FROM MemberProfilesOld where id > " + nextId + "  order by id asc ");
                    var took = (DateTime.Now - startTime).TotalSeconds;
                    Console.WriteLine($"Starts from Id : {nextId}");
                    Console.WriteLine($"Processed items : {rowsInserted}");
                    Console.WriteLine($"Total seconds took: {took}");
                    Console.WriteLine($"");
                    if (rowsInserted == 0)
                    {
                        Console.WriteLine($"No more rows");
                        stopProcessing = true;
                        break;
                    }
                }

            }
            Console.WriteLine($"===============================================================================");
            Console.WriteLine("");
            Console.WriteLine("Completed CopyMemberProfileToNewTableCommand.Execute");
            var a = 0;
            return;
        }
    }
}