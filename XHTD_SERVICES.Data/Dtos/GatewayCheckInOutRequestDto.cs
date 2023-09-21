using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES.Data.Dtos
{
    public class GatewayCheckInOutRequestDto
    {
        public string RfId { get; set; }

        public DateTime? CheckTime { get; set; }

        public FileDto File { get; set; }
    }
}
