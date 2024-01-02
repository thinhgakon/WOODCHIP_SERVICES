using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES_SYNC_BRAVO.Models.Request
{
    public class ScaleBillDto
    {
        public string Code { get; set; }
        public string OrderCode { get; set; }
        public string ScaleTypeCode { get; set; }
        public string PartnerCode { get; set; }
        public string VehicleCode { get; set; }
        public string DriverName { get; set; }
        public string ItemCode { get; set; }
        public string Note { get; set; }
        public Nullable<double> Weight1 { get; set; }
        public Nullable<double> Weight2 { get; set; }
    }
}
