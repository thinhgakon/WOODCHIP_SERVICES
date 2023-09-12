using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES.Data.Models.Response;
using log4net;
using System.Data.Entity;

namespace XHTD_SERVICES.Data.Repositories
{
    public class ScaleBillRepository : BaseRepository <ScaleBill>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScaleBillRepository(XHTD_Entities appDbContext) : base(appDbContext)
        {
        }

        public List<ScaleBillDto> GetList()
        {
            using (var dbContext = new XHTD_Entities())
            {
                try { 
                    var items = dbContext.ScaleBills
                        .Include(x => x.MdItem)
                        .Include(x => x.MdPartner)
                        .Where(x => x.IsSynced == false)
                        .ToList()
                        .Select(x => new ScaleBillDto
                    {
                        Code = x.Code,
                        CompanyCode = "VIJACHIP",
                        ScaleTypeCode = x.ScaleTypeCode,
                        PartnerCode = x.MdPartner.SyncCode,
                        VehicleCode = x.VehicleCode,
                        DriverName = x.DriverName,
                        ItemCode = x.MdItem.SyncCode,
                        Note = x.Note,
                        Weight1 = x.Weight1,
                        Weight2 = x.Weight2,
                        TimeWeight1 = x.TimeWeight1,
                        TimeWeight2 = x.TimeWeight2,
                        //StationCode = x.StationCode,
                        //AreaCodes = x.AreaCode,
                    })
                    .ToList();

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
