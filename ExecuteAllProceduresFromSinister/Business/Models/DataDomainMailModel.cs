using System;

namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class DataDomainMailModel : DataActionMailModel
    {
        public string ContainMail { get; set; }
        public string PatternRegex { get; set; }
        public bool StrictStart { get; set; }

        public DataDomainMailModel(string subject, Func<string, string, string> funcSubject, string containMail = null, string patternRegex = null, bool onlyLoad = false, bool isGenericTask = false, bool strictStart = false) : base(subject, funcSubject, onlyLoad, isGenericTask)
        {
            ContainMail = containMail;
            PatternRegex = patternRegex;
            StrictStart = strictStart;
        }
    }
}
