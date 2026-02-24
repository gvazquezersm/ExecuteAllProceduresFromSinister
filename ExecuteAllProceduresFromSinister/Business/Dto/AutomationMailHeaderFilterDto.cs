using System;
using System.Collections.Generic;

namespace ExecuteAllProceduresFromSinister.Business.Dto
{
    public class AutomationMailHeaderFilterDto
    {
        public string OriginMail { get; set; }
        public string ToMail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IEnumerable<FileDto> Attachments { get; set; }
        public DateTime DateTimeReceived { get; set; }
        public bool IsHtml { get; set; }
        public string CC { get; set; }
    }
}
