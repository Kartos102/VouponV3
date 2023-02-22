using System;
using System.Collections.Generic;
using System.Text;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Common.RedemptionTokenGenerator
{
    public class InStoreRedemptionToken
    {     
        //Generate Token with
        // return {First Phase - Id convert to base 36} + {Second Phase - Random Number convert to base 36}
        public static string GenerateToken(int id)
        {
            Random _random = new Random();
            string firstSection = ToBase36(id);
            int num = _random.Next(1679615);
            string secondSection = ToBase36(num);
            int totalPadding = 4 - secondSection.Length;
            secondSection = secondSection.PadLeft(4, '0');
            string token = firstSection + secondSection;
            return token;
        }

        private static string ToBase36(int value)
        {
            const string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder(13);
            do
            {
                sb.Insert(0, base36[(byte)(value % 36)]);
                value /= 36;
            } while (value != 0);
            return sb.ToString();
        }        
    }
}
