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
        private static readonly ILog log = LogManager.GetLogger("SecondFileAppender");

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
                        .Take(100)
                        .Select(x => new ScaleImageDto
                    {
                        Id = x.Id,
                        ScaleBillCode = x.ScaleBillCode,
                        AttachmentId = x.AttachmentId,
                        Type = x.Type,
                        Attachment = new AttachmentDto
                        {
                            Title = x.Attachment.Title,
                            Url = x.Attachment.Url,
                            Extension = x.Attachment.Extension,
                        },
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

        public async Task<bool> UpdateSyncSuccess(string id)
        {
            using (var dbContext = new XHTD_Entities())
            {
                try
                {
                    string syncTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    var order = await dbContext.ScaleImages
                                            .Where(x => x.Id.ToString() == id)
                                            .FirstOrDefaultAsync();

                    if (order == null)
                    {
                        return false;
                    }

                    order.IsSynced = true;
                    order.SyncDate = DateTime.Now;
                    if (order.SyncLog.Length > 800)
                    {
                        order.SyncLog = $@"#Đồng bộ lúc {syncTime} ";
                    }
                    else
                    {
                        order.SyncLog = $@"{order.SyncLog} #Đồng bộ lúc {syncTime} ";
                    }

                    await dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error($@"Sync {id} Error: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
                    return false;
                }
            }
        }

        public async Task<bool> UpdateSyncFail(string id)
        {
            using (var dbContext = new XHTD_Entities())
            {
                try
                {
                    string syncTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    var order = await dbContext.ScaleImages
                                            .Where(x => x.Id.ToString() == id)
                                            .FirstOrDefaultAsync();

                    if (order == null)
                    {
                        return false;
                    }

                    order.SyncDate = DateTime.Now;
                    if(order.SyncLog.Length > 800)
                    {
                        order.SyncLog = $@"#Đồng bộ thất bại lúc {syncTime} ";
                    }
                    else { 
                        order.SyncLog = $@"{order.SyncLog} #Đồng bộ thất bại lúc {syncTime} ";
                    }

                    await dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error($@"Sync {id} Error: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
                    return false;
                }
            }
        }
    }
}
