using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES_SYNC_BRAVO.Models.Response
{
    public class SyncResponse
    {
        public List<DataSyncResponse> success { get; set; }
        public List<DataSyncResponse> fails { get; set; }
    }
}
