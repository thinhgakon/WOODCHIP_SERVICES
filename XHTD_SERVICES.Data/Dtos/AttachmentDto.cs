using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES.Data.Dtos
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string Thumbnail { get; set; }

        public string Extension { get; set; }

        public double Size { get; set; }

        public string Type { get; set; }
    }
}
