using System;

namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class DataActionMailModel
    {
        public string Subject { get; set; }
        public Func<string, string, string> FuncSubject { get; set; }
        public bool OnlyLoad { get; set; }
        public bool IsGenericTask { get; set; }

        public DataActionMailModel(string subject, Func<string, string, string> funcSubject, bool onlyLoad = false, bool isGenericTask = false)
        {
            Subject = subject;
            FuncSubject = funcSubject;
            OnlyLoad = onlyLoad;
            IsGenericTask = isGenericTask;
        }

        public DataActionMailModel() { }
    }
}
