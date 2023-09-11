using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES.Data.Models.Response;
using log4net;

namespace XHTD_SERVICES.Data.Repositories
{
    public class ScaleBillRepository : BaseRepository <ScaleBill>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScaleBillRepository(XHTD_Entities appDbContext) : base(appDbContext)
        {
        }

        public List<ScaleBill> GetList()
        {
            using (var dbContext = new XHTD_Entities())
            {
                try { 
                    var items = dbContext.ScaleBills.ToList();
                    if (items == null)
                    {
                        return null;
                    }

                    return items;
                }
                catch(Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
