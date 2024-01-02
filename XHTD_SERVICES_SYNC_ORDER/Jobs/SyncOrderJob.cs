using System;
using System.Threading.Tasks;
using Quartz;
using XHTD_SERVICES.Data.Entities;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using XHTD_SERVICES.Helper;

namespace XHTD_SERVICES_SYNC_ORDER.Jobs
{
    public class SyncOrderJob : IJob
    {
        private readonly mmes_bravoEntities _bravoContext;
        private readonly XHTD_Entities _mMesContext;
        protected readonly SyncOrderLogger _syncOrderLogger;

        public SyncOrderJob(mmes_bravoEntities bravoContext, XHTD_Entities mMesContext, SyncOrderLogger syncOrderLogger)
        {
            _bravoContext = bravoContext;
            _mMesContext = mMesContext;
            _syncOrderLogger = syncOrderLogger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await Task.Run(async () =>
            {
                await SyncOrderProcess();
            });
        }

        public async Task SyncOrderProcess()
        {
            _syncOrderLogger.LogInfo("===================================------------------===================================");
            _syncOrderLogger.LogInfo("Start process Sync Order job");
            Console.WriteLine("Start process Sync Order job");
            var unSyncBills = await _mMesContext.ScaleBills.Include(x => x.MdPartner).Include(x => x.MdArea).Where(x => !x.IsSyncToBravo).ToListAsync();

            try
            {
                _bravoContext.Database.BeginTransaction();
                _mMesContext.Database.BeginTransaction();
                foreach (var x in unSyncBills)
                {
                    var bravoBill = new Weightman()
                    {
                        Trantype = x.ScaleTypeCode,
                        Custcode = x.PartnerCode,
                        Custname = x.MdPartner.Name,
                        Truckno = x.VehicleCode,
                        Note = x.CreateDate ?? DateTime.Now,
                        Firstweight = Convert.ToDecimal(x.Weight1),
                        Secondweight = Convert.ToDecimal(x.Weight2),
                        Date_in = x.TimeWeight1.Value.Date,
                        Date_out = x?.TimeWeight2?.Date,
                        time_in = x.TimeWeight1.Value.TimeOfDay,
                        time_out = x?.TimeWeight2?.TimeOfDay,
                        NetWeight = Convert.ToDecimal(x.Weight),
                        Prodcode = x?.AreaCode,
                        Prodname = x?.MdArea?.Name,
                        sp = x.BillNumber,
                        sohd = x.InvoiceNumber,
                        mauhd = x.InvoiceTemplate,
                        Docnum = x.InvoiceSymbol,
                        Id = x?.BravoId ?? 0
                    };

                    _bravoContext.Weightmen.AddOrUpdate(bravoBill);

                    x.IsSyncToBravo = true;
                    if (x.BravoId == null || x.BravoId == 0)
                    {
                        x.BravoId = bravoBill.Id;
                        _mMesContext.ScaleBills.AddOrUpdate(x);
                    }
                    Console.WriteLine($"Sync {x.Code}");
                }
                await _mMesContext.SaveChangesAsync();
                await _bravoContext.SaveChangesAsync();

                _bravoContext.Database.CurrentTransaction.Commit();
                _mMesContext.Database.CurrentTransaction.Commit();
            }
            catch (Exception ex)
            {
                _syncOrderLogger.LogInfo(ex.Message);
                _syncOrderLogger.LogInfo(ex.StackTrace);
                _bravoContext.Database.CurrentTransaction.Rollback();
                _mMesContext.Database.CurrentTransaction.Rollback();
            }
        }
    }
}
