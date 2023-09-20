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
using XHTD_SERVICES.Data.Dtos;

namespace XHTD_SERVICES.Data.Repositories
{
    public class ScaleImageRepository : BaseRepository <ScaleBill>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScaleImageRepository(XHTD_Entities appDbContext) : base(appDbContext)
        {
        }

        public List<ScaleImageDto> GetList()
        {
            using (var dbContext = new XHTD_Entities())
            {
                try { 
                    var items = dbContext.ScaleImages
                        .Where(x => x.IsSynced == null || x.IsSynced == false)
                        .ToList()
                        .Select(x => new ScaleImageDto
                    {
                        ScaleBillCode = x.ScaleBillCode,
                        AttachmentId = x.AttachmentId,
                        Type = x.Type,
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
