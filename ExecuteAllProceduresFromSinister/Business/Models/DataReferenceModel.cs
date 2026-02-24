namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class DataReferenceModel
    {
        public string Reference { get; set; }
        public bool IsSinAlias { get; set; }
        public bool Resent { get; set; }
        public bool IsGenericTask { get; set; }
        public bool OnlyLoad { get; set; }
    }
}
