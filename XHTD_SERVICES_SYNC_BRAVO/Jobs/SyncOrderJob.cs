using System;
using System.Threading.Tasks;
using Quartz;
using XHTD_SERVICES.Data.Entities;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using XHTD_SERVICES.Helper;

namespace XHTD_SERVICES_SYNC_BRAVO.Jobs
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
            _syncOrderLogger.LogInfo("===================================- Start process Sync Order job -===================================");
            var unSyncBills = await _mMesContext.ScaleBills.Include(x => x.MdPartner).Include(x => x.MdArea).Where(x => !x.IsSyncToBravo).ToListAsync();

            try
            {
                foreach (var x in unSyncBills)
                {
                    if (x.IsCanceled == true)
                    {
                        x.IsSyncToBravo = true;
                        var _bravoBills = await _bravoContext.Weightmen.Where(br => br.ScaleBillCode == x.Code).ToListAsync();
                        _bravoContext.Weightmen.RemoveRange(_bravoBills);
                        await _bravoContext.SaveChangesAsync();
                        await _mMesContext.SaveChangesAsync();
                    }

                    else
                    {
                        var bravoBill = new Weightman()
                        {
                            ID = x?.BravoId ?? 0,
                            Trantype = GetScaleType(x.ScaleTypeCode),
                            Custcode = GetCustCode(x.PartnerCode),
                            Custname = x.MdPartner.Name,
                            Truckno = x.VehicleCode,
                            Note = x.CreateDate ?? DateTime.Now,
                            Firstweight = Convert.ToDecimal(x.Weight1),
                            Secondweight = Convert.ToDecimal(x.Weight2),
                            Date_in = x.TimeWeight1.Value.Date,
                            Date_out = x?.TimeWeight2?.Date,
                            time_in = x.TimeWeight1.Value.TimeOfDay,
                            time_out = x?.TimeWeight2?.TimeOfDay,
                            Netweight = Convert.ToDecimal(x.Weight),
                            Prodcode = GetProdcode(x?.AreaCode),
                            Prodname = x?.MdArea?.Name,
                            Ticketnum = int.TryParse(x.BillNumber, out int i) ? i : 0,
                            sohd = x.InvoiceNumber,
                            mauhd = x.InvoiceTemplate,
                            Docnum = x.InvoiceSymbol,
                            date_time = x.TimeWeight2,
                            Netweight2 = Convert.ToDecimal(x.Weight),
                            ScaleBillCode = x.Code
                        };

                        var existed = await _bravoContext.Weightmen.FirstOrDefaultAsync(br => br.ScaleBillCode == x.Code);
                        if (existed == null)
                        {
                            _bravoContext.Weightmen.Add(bravoBill);
                        }
                        else
                        {
                            _bravoContext.Entry(existed).CurrentValues.SetValues(bravoBill);
                            _bravoContext.Entry(existed).State = EntityState.Modified;
                        }

                        var result = await _bravoContext.SaveChangesAsync();
                        if (result > 0 && bravoBill.ID > 0)
                        {
                            x.IsSyncToBravo = true;
                            x.BravoId = bravoBill.ID;
                            _mMesContext.ScaleBills.AddOrUpdate(x);
                            Console.WriteLine($"Sync {x.Code}");
                            await _mMesContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _syncOrderLogger.LogInfo(ex.Message);
                _syncOrderLogger.LogInfo(ex.StackTrace);
            }
        }
        private string GetScaleType(string scaleTypeCode)
        {
            if(scaleTypeCode == "NHAP_HANG")
            {
                return "NHẬP HÀNG";
            }

            return "XUẤT HÀNG";
        }

        private string GetCustCode(string partnerCode)
        {
            switch (partnerCode)
            {
                case "DN-VNF-HQ": return "DN-VNF-HQ";
                case "VP": return "DAMBUI2";
                case "VJC": return "VJC3";
                case "TH": return "CTTH";
                case "SCHK": return "SCHK.";
                case "RENEN": return "TCT-RENEN";
                case "Qnafor.FM": return "QNAFOR.FM";
                case "QN": return "QN";
                case "NDMT": return "DAMNGADOANH";
                case "KL02": return "KL02";
                case "KHL": return "KHL";
                case "KH": return "KHANHHAN";
                case "HTH": return "HTH";
                case "HNV": return "DAMBUI3";
                case "HN": return "CTHN";
                default: return partnerCode; 
            }
        }

        private string GetProdcode(string areaCode)
        {
            switch (areaCode)
            {
                case "KTCDX": return "KTCDX";
                case "BBDD": return "KDDBB";
                case "BBTCDD": return "BBTCDD";
                case "DD": return "KDD";
                case "DGTG": return "KDGTG";
                case "DK": return "KDK";
                case "DL": return "KDL";
                case "DLVB": return "KDL-VB";
                case "DN": return "KDN";
                case "HN-NG": return "KHN-NG";
                case "HN-PS": return "KHN-PS";
                case "QTR": return "KQTR";
                case "QND": return "KQND";
                case "H-QTR-QB-HT": return "KH-QTR-QB-HT";
                default: return areaCode;
            }
        }
    }
}
