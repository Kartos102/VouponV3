﻿using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Ip2nationCountries
    {
        public Ip2nationCountries()
        {
            CommercialRates = new HashSet<CommercialRates>();
            Ip2Nations = new HashSet<Ip2Nations>();
        }

        public string Code { get; set; }
        public string IsoCode2 { get; set; }
        public string IsoCode3 { get; set; }
        public string IsoCountry { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CurrencyCode { get; set; }

        public virtual ICollection<CommercialRates> CommercialRates { get; set; }
        public virtual ICollection<Ip2Nations> Ip2Nations { get; set; }
    }
}
