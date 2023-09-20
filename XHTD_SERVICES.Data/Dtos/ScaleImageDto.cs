using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XHTD_SERVICES.Data.Dtos
{
    public class ScaleImageDto
    {
        public System.Guid Id { get; set; }

        public string ScaleBillCode { get; set; }

        public Guid AttachmentId { get; set; }

        public string Type { get; set; }

        public virtual AttachmentDto Attachment { get; set; }
    }
}
