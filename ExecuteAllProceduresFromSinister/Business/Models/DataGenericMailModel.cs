namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class DataGenericMailModel<TData>
    {
        public string Case { get; set; }
        public TData Data { get; set; }
        /// <summary>
        /// Dominio canónico registrado en ERSM para la búsqueda por SinRefCompany.
        /// Usar cuando el dominio del remitente es un subdominio (ej. email.fiatc.es)
        /// pero ERSM tiene registrado el dominio raíz (ej. fiatc.es).
        /// Si es null, se usa el OriginMail original del correo.
        /// </summary>
        public string CanonicalDomain { get; set; }
    }
}
