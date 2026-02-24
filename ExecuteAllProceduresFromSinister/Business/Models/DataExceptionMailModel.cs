using ExecuteAllProceduresFromSinister.Business.Dto;
using System;
using System.Collections.Generic;

namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class DataExceptionMailModel
    {
        public string BodyCase { get; set; }
        public string SubjectCase { get; set; }
        public string FileTextCase { get; set; }
        public bool IsCheckBody { get; set; }
        public bool IsCheckAttachments { get; set; }
        public Func<string, string, bool> CheckBody { get; set; }
        public Func<IEnumerable<FileDto>, string, bool> CheckAttachments { get; set; }
    }
}
