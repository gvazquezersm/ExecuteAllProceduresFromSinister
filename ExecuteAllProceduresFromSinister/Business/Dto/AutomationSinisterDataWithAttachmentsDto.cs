using System;
using System.Collections.Generic;

namespace ExecuteAllProceduresFromSinister.Business.Dto
{
    public class AutomationSinisterDataWithAttachmentsDto : AutomationSinisterDataDto
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromMail { get; set; }
        public bool IsBodyHtml { get; set; }
        public IEnumerable<string> ToMails { get; set; }
        public IEnumerable<FileDto> Attachments { get; set; }
        public DateTime DateTimeReceived { get; set; }
        public IEnumerable<string> CCMails { get; set; }
    }
}
