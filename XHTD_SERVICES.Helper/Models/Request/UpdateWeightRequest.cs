﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES.Helper.Models.Request
{
    public class UpdateWeightRequest
    {
        public string deliveryCode { get; set; }
        public double weight { get; set; }
    }
}
