using System.Threading.Tasks;
using XHTD_SERVICES.Data.Entities;
using log4net;
using XHTD_SERVICES.Data.Dtos;
using System.Data.Entity;

namespace XHTD_SERVICES.Data.Repositories
{
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
                if(partner == null)
                {
                    return null;
                }

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
