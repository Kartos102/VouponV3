using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp
{
    public class Version
    {
        #region Constants
        private const string NAME = "Voupon";
        private const string COPYRIGHT = "Copyright 2020, Vodus";
        private const string VERSION = "0.0.1 (June 2020)";
        #endregion
        /* 
         * v0.0.1 (29th June 2020)
         * - Initial setup
        */
        public static string GetName() { return NAME; }
        public static string GetCopyright() { return COPYRIGHT; }
        public static string GetVersion() { return GetVersion(false); }
        public static string GetVersion(bool bNoDate)
        {
            var indexOfCurly = VERSION.IndexOf('(');
            return bNoDate ? VERSION.Substring(0, indexOfCurly < 0 ? 5 : indexOfCurly) : VERSION;
        }

        public static int VersionStringToInt(string strVersion)
        {
            if (string.IsNullOrEmpty(strVersion))
                return -1;

            var numericVersion = strVersion.Replace(".", string.Empty);
            int intVersion;
            if (Int32.TryParse(numericVersion, out intVersion))
                return intVersion;

            return -1;
        }

        public new static string ToString()
        {
            return string.Format("{0} ({1})\nv{2} {3}", NAME,
#if DEBUG
 "Debug Build",
#else
 "Release Build",
#endif
 VERSION, COPYRIGHT);
        }

    }
}
