namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class DataGenericMailModel<TData>
    {
        public string Case { get; set; }
        public TData Data { get; set; }
    }
}
