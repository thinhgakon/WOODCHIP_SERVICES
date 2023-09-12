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
using XHTD_SERVICES.Data.Models.Values;

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
                        .Include(x => x.MdArea)
                        .Where(x => x.IsSynced == null || x.IsSynced == false)
                        .ToList()
                        .Select(x => new ScaleBillDto
                    {
                        Code = x.Code,
                        CompanyCode = "VJ",
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
                        AreaCode = x.MdArea.SyncCode,
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

        public async Task<bool> UpdateSyncSuccess(string code)
        {
            using (var dbContext = new XHTD_Entities())
            {
                try
                {
                    string syncTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    var order = await dbContext.ScaleBills
                                            .Where(x => x.Code == code)
                                            .FirstOrDefaultAsync();

                    if (order == null)
                    {
                        return false;
                    }

                    order.IsSynced = true;
                    order.SyncDate = DateTime.Now;
                    order.SyncLog = $@"{order.SyncLog} #Đồng bộ lúc {syncTime} ";

                    await dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error($@"Sync {code} Error: " + ex.Message);
                    return false;
                }
            }
        }

        public async Task<bool> UpdateSyncFail(string code)
        {
            using (var dbContext = new XHTD_Entities())
            {
                try
                {
                    string syncTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    var order = await dbContext.ScaleBills
                                            .Where(x => x.Code == code)
                                            .FirstOrDefaultAsync();

                    if (order == null)
                    {
                        return false;
                    }

                    order.SyncDate = DateTime.Now;
                    order.SyncLog = $@"{order.SyncLog} #Đồng bộ thất bại lúc {syncTime} ";

                    await dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error($@"Sync {code} Error: " + ex.Message);
                    return false;
                }
            }
        }
    }
}
