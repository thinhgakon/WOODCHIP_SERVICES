using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using XHTD_SERVICES.Data.Entities;
using XHTD_SERVICES.Data.Models.Response;
using log4net;
using XHTD_SERVICES.Data.Dtos;
using System.Data.Entity;

namespace XHTD_SERVICES.Data.Repositories
{
    public class RfidRepository : BaseRepository <MdRfid>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public RfidRepository(XHTD_Entities appDbContext) : base(appDbContext)
        {
        }

        public bool CheckValidCode(string code)
        {
            bool isValid = false;
            using (var dbContext = new XHTD_Entities())
            {
                isValid = dbContext.MdRfids.Any(x => x.Code == code.Trim());
            }

            //if (code.Trim().StartsWith("999"))
            //{
            //    using (var dbContext = new XHTD_Entities())
            //    {
            //        isValid = dbContext.tblRfids.Any(x => x.Code == code.Trim());
            //    }
            //}

            return isValid;
        }

        public string GetRfidbyVehicle(string vehicle)
        {
            using (var dbContext = new XHTD_Entities())
            {
                var rfidRecord = dbContext.MdRfids.FirstOrDefault(x => x.VehicleCode == vehicle);
                if (rfidRecord == null)
                {
                    return null;
                }

                if (String.IsNullOrEmpty(rfidRecord.Code))
                {
                    return null;
                }

                return rfidRecord.Code;
            }
        }
    }

    public class PartnerRepository : BaseRepository<MdPartner>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PartnerRepository(XHTD_Entities appDbContext) : base(appDbContext)
        {
        }
        public async Task<PartnerDto> GetPartner(string code)
        {
            using (var dbContext = new XHTD_Entities())
            {
                var partner = await dbContext.MdPartners.FirstOrDefaultAsync(x => x.Code == code);

                return new PartnerDto()
                {
                    Code = partner.Code,
                    Address = partner.Address,
                    Email = partner.Email,
                    IsCustomer = partner.IsCustomer,
                    IsProvider = partner.IsProvider,
                    Name = partner.Name,
                    PhoneNumber = partner.PhoneNumber,
                    TaxCode = partner.TaxCode
                };
            }
        }
    }
}
