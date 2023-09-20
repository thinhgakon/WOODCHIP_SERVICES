using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES.Data.Dtos
{
    public class ScaleBillRequestDto {
        public ScaleBillRequestDto(ScaleBillDto dto)
        {
            this.Code= dto.Code;
            this.CompanyCode= dto.CompanyCode;
            this.ScaleTypeCode= dto.ScaleTypeCode;
            this.PartnerCode= dto.PartnerCode;  
            this.VehicleCode= dto.VehicleCode;
            this.DriverName= dto.DriverName;
            this.ItemCode= dto.ItemCode;
            this.Note= dto.Note;
            this.Weight1 = dto.Weight1;
            this.Weight2 = dto.Weight2;
            this.AreaCode = dto.AreaCode;
            this.TimeWeight1 = dto.TimeWeight1?.ToString("s");
            this.TimeWeight2 = dto.TimeWeight2?.ToString("s");
            this.UnitCode = dto.UnitCode;
        }

        public string Code { get; set; }

        public string CompanyCode { get; set; }

        public string ScaleTypeCode { get; set; }

        public string PartnerCode { get; set; }

        public string VehicleCode { get; set; }

        public string DriverName { get; set; }

        public string ItemCode { get; set; }

        public string Note { get; set; }

        public double? Weight1 { get; set; }

        public double? Weight2 { get; set; }

        public string TimeWeight1 { get; set; }

        public string TimeWeight2 { get; set; }

        public string AreaCode { get; set; }

        public string UnitCode { get; set; }
    }
}
