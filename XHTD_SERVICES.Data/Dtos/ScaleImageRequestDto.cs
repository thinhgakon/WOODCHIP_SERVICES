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

        public string SyncCode { get; set; }

        public string ScaleCode { get; set; }

        public FileDto Files { get; set; }
    }
}
