using ExecuteAllProceduresFromSinister.Common.Enums;
using System;

namespace ExecuteAllProceduresFromSinister.Business.Dto
{
    public class AddLogsLogicAppDto
    {
        public string OriginMail { get; set; }
        public string Subject { get; set; }
        public string Observations { get; set; }
        public TipologyMailEnum Tipology { get; set; }
    }
}
