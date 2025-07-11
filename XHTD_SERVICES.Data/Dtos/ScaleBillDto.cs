﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES.Data.Dtos
{
    public class ScaleBillDto
    {
        public string Code { get; set; }

        public string CompanyCode { get; set; }

        public string ScaleTypeCode { get; set; }

        public string PartnerCode { get; set; }

        public string Rfid { get; set; }

        public string VehicleCode { get; set; }

        public string DriverName { get; set; }

        public string ItemCode { get; set; }

        public string Note { get; set; }

        public double? Weight1 { get; set; }

        public double? Weight2 { get; set; }

        public DateTime? TimeWeight1 { get; set; }

        public DateTime? TimeWeight2 { get; set; }

        public string StationCode { get; set; }

        public string AreaCode { get; set; }

        public string UnitCode { get; set; } = "KG";

        public string BillNumber { get; set; }

        public string InvoiceNumber { get; set; }

        public string InvoiceTemplate { get; set; }

        public string InvoiceSymbol { get; set; }

        public bool? IsCanceled { get; set; }
    }
}
