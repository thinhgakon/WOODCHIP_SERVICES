﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES_TRAM951_1.Models.Request
{
    public class GetTokenRequest
    {
        public string grant_type { get; set; }
        public string client_secret { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string client_id { get; set; }
    }
}
