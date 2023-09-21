using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES_GATEWAY.Models.Response
{
    public class CheckInOutResponse
    {
        public string VehicleCode { get; set; }

        public string CheckInTime { get; set; }

        public string CheckOutTime { get; set; }

        public string RfId { get; set; }
    }
}
