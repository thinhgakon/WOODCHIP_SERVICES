using System;

namespace XHTD_SERVICES.Data.Dtos
{
    public class PartnerDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public bool IsCustomer { get; set; }

        public bool IsProvider { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string TaxCode { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public bool IsActiver { get => true; }
    }

}
