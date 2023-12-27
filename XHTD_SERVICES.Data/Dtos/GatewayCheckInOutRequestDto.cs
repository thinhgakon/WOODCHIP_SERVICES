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

        public DateTime? CheckTimeDateTimeValue { get; set; }

        public string CheckTime { get => CheckTimeDateTimeValue?.ToString("s"); }

        public List<FileDto> Files { get; set; }
    }
}
