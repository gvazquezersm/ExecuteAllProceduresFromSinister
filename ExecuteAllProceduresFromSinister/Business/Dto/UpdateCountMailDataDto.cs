using ExecuteAllProceduresFromSinister.Common.Enums;

namespace ExecuteAllProceduresFromSinister.Business.Dto
{
    public class UpdateCountMailDataDto
    {
        public TipologyMailEnum Tipology { get; set; }
        public UpdateCountMailTypeEnum UpdateType { get; set; }
    }
}
