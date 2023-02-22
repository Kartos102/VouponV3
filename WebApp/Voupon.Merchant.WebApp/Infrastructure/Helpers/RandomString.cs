using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Infrastructures.Helpers
{
    public class RandomString
    {
        //private static Random random = new Random((int)DateTime.Now.Ticks);//thanks to McAden
        private static Random random = new Random((int)DateTime.Now.Ticks);
        public static string GenerateRandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
