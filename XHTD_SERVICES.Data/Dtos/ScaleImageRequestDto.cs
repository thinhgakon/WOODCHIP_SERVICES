using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES.Data.Dtos
{
    public class ScaleImageRequestDto
    {
        public string ModuleType { get; set; }

        public string Code { get; set; }

        public string ScaleCode { get; set; }

        public byte[] File { get; set; }
    }
}
