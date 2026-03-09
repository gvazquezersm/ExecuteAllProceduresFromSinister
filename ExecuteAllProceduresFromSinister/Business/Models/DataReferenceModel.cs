namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class DataReferenceModel
    {
        public string Reference { get; set; }
        public bool IsSinAlias { get; set; }
        public bool Resent { get; set; }
        public bool IsGenericTask { get; set; }
        public bool OnlyLoad { get; set; }
        /// <summary>
        /// OriginMail normalizado para la búsqueda en ERSM.
        /// Se usa cuando el dominio del remitente es un subdominio no registrado en ERSM
        /// (ej. fiatc@email.fiatc.es → fiatc@fiatc.es).
        /// Si es null, se usa el OriginMail original del correo.
        /// </summary>
        public string LookupOriginMail { get; set; }
    }
}
